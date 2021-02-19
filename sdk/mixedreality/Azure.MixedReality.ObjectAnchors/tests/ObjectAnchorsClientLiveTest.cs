﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Core.TestFramework;
using Azure.Identity;
using Azure.MixedReality.Authentication;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using NUnit.Framework;

namespace Azure.MixedReality.ObjectAnchors.Tests
{
    public class ObjectAnchorsClientLiveTest : RecordedTestBase<ObjectAnchorsClientTestEnvironment>
    {
        public ObjectAnchorsClientLiveTest(bool isAsync)
            : base(isAsync, RecordedTestMode.Record)
        {
            //TODO: https://github.com/Azure/autorest.csharp/issues/689
            TestDiagnostics = false;

            Matcher = new MixedRealityRecordMatcher();
        }

        [RecordedTest]
        public async Task RunIngestion()
        {
            Recording.DisableIdReuse();
            Guid accountId = new Guid(TestEnvironment.AccountId);
            string accountDomain = TestEnvironment.AccountDomain;
            ObjectAnchorsClientOptions options = new ObjectAnchorsClientOptions();
            options.MixedRealityAuthenticationOptions = InstrumentClientOptions(new MixedRealityStsClientOptions());
            string localFilePath = TestEnvironment.AssetLocalFilePath;
            Vector3 assetGravity = new Vector3(TestEnvironment.AssetGravityX, TestEnvironment.AssetGravityY, TestEnvironment.AssetGravityZ);
            float scale = TestEnvironment.AssetScale;

            AzureKeyCredential credential = new AzureKeyCredential(TestEnvironment.AccountKey);

            ObjectAnchorsClient client = InstrumentClient(new ObjectAnchorsClient(accountId, accountDomain, credential, InstrumentClientOptions(options)));

            Uri uploadedInputAssetUri = (await client.GetAssetUploadUriAsync()).Value.UploadUri;

            BlobClient uploadBlobClient = InstrumentClient(new BlobClient(uploadedInputAssetUri, InstrumentClientOptions(new BlobClientOptions())));

            using (FileStream fs = File.OpenRead(localFilePath))
            {
                await uploadBlobClient.UploadAsync(fs);
            }

            AssetConversionOptions ingestionJobOptions = new AssetConversionOptions(uploadedInputAssetUri, AssetFileType.FromFilePath(localFilePath), assetGravity, scale);

            ingestionJobOptions.JobId = Recording.Random.NewGuid();

            AssetConversionOperation operation = await client.StartAssetConversionAsync(ingestionJobOptions);

            Guid jobId = new Guid(operation.Id);

            await operation.WaitForCompletionAsync();

            if (!operation.HasCompletedSuccessfully)
            {
                throw new Exception("The asset conversion operation completed with an unsuccessful status");
            }

            string localFileDownloadPath = TestEnvironment.ModelDownloadLocalFilePath;

            BlobClient downloadBlobClient = InstrumentClient(new BlobClient(operation.Value.OutputModelUri, InstrumentClientOptions(new BlobClientOptions())));

            BlobDownloadInfo downloadInfo = await downloadBlobClient.DownloadAsync();

            using (FileStream file = File.OpenWrite(localFileDownloadPath))
            {
                await downloadInfo.Content.CopyToAsync(file);
                var fileInfo = new FileInfo(localFileDownloadPath);
                Assert.Greater(fileInfo.Length, 0);
            }
        }
    }
}
