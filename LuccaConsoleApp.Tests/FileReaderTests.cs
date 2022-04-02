using Xunit;
using LuccaConsoleApp;
using System;

namespace LuccaConsoleApp.Tests
{
    public class FileReaderTests
    {
        //[Fact]
        //public void Initialise_WithEmptyArray()
        //{
        //    Assert.Null(new RequestProcessor(new string[]{ }));
        //}
        //[Fact]
        //public void Initialise_WithOneLine()
        //{
        //    Assert.Null(new RequestProcessor(new string[] { "EUR;50;JPY" }));
        //}

        [Fact]
        public void Initialise_InitialisingWithoutEnoughLine_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => new RequestProcessor( new string[] { "EUR;50;JPY" }));
        }
    }
}