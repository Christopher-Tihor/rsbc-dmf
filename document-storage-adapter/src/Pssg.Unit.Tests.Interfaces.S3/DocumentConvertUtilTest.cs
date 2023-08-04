﻿using Pssg.DocumentStorageAdapter;
using System.IO;
using Xunit;

namespace Pssg.Unit.Tests.Interfaces.S3
{
    public class DocumentConvertUtilTest
    {
        /// <summary>
        /// Convert Tiff 2Pdf Test
        /// </summary>
        [Fact]
        public void ConvertTiff2PdfTest()
        {
            //Arrange
            // Get TiffImage as bytes array
            byte[] imageAsByteStream = File.ReadAllBytes("test.tiff");

            // Act 
            // and then send it to convertTiff2Pdf function 
            var pdfBytes = DocumentConvertUtil.convertTiff2Pdf(imageAsByteStream);

            File.WriteAllBytes("test.pdf", pdfBytes);

            // Asset 
            // check result is of type bytes 
            // check result bytes length  
            Assert.True(pdfBytes.Length > 0);
        }
    }
}
