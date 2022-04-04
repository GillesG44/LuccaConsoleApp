using Xunit;
using LuccaConsoleApp;
using System;

namespace LuccaConsoleApp.Tests
{
    public class FileReaderTests
    {
        [Fact]
        public void Initialise_WithEmptyArray_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new RequestProcessor(Array.Empty<string>()));
        }

        [Fact]
        public void Initialise_InitialisingWithoutEnoughLine_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new RequestProcessor( new string[] { "EUR;50;JPY" }));
        }

        [Fact]
        public void Initialise_DefaultTest_Success()
        {
            var textEntry = new string[] { "EUR;550;JPY", "6", "AUD;CHF;0.9661", "JPY;KRW;13.1151", "EUR;CHF;1.2053", "AUD;JPY;86.0305", "EUR;USD;1.2989", "JPY;INR;0.6571" };
            Assert.Equal(59033, new RequestProcessor(textEntry).CalculateBestRate());
        }

        [Fact]
        public void Initialise_WithWhitespaces_Success()
        {
            var textEntry = new string[] { "EUR;\t\n550;   JPY", "6", "AUD;CHF;0.9661\t\t", "JPY;KRW;          13.1151", "EUR\n;CHF;1.2053", "\tAUD;JPY;86.0305", "EUR;USD;1.2989", "JPY;INR;0.6571" };
            Assert.Equal(59033, new RequestProcessor(textEntry).CalculateBestRate());
        }

        [Fact]
        public void Initialise_WrongNumberOfCharactersForCurrency_ThrowsArgumentException()
        {
            var textEntry = new string[] { "EURO;550;JPY", "6", "AUD;CHF;0.9661", "JPY;KRW;13.1151", "EUR;CHF;1.2053", "AUD;JPY;86.0305", "EUR;USD;1.2989", "JPY;INR;0.6571" };
            Assert.Throws<ArgumentException>(() => new RequestProcessor(textEntry).CalculateBestRate());
        }

        [Fact]
        public void Initialise_WithSameSourceAndTarget_DontChangeAmount()
        {
            var textEntry = new string[] { "EUR;550;EUR", "6", "AUD;CHF;0.9661", "JPY;KRW;13.1151", "EUR;CHF;1.2053", "AUD;JPY;86.0305", "EUR;USD;1.2989", "JPY;INR;0.6571" };
            Assert.Equal(550, new RequestProcessor(textEntry).CalculateBestRate());
        }

        [Fact]
        public void Initialise_Initialise_WithMoreConversions_ThrowsArgumentException()
        {
            var textEntry = new string[] { "EUR;550;JPY", "6", "AUD;CHF;0.9661", "JPY;KRW;13.1151", "EUR;CHF;1.2053", "AUD;JPY;86.0305", "EUR;USD;1.2989", "JPY;INR;0.6571", "JPY;INR;0.6571" };
            Assert.Throws<ArgumentException>(() => new RequestProcessor(textEntry).CalculateBestRate());
        }
        [Fact]
        public void Initialise_Initialise_WithLessConversions_ThrowsArgumentException()
        {
            var textEntry = new string[] { "EUR;550;JPY", "6", "AUD;CHF;0.9661", "JPY;KRW;13.1151", "EUR;CHF;1.2053", "AUD;JPY;86.0305", "EUR;USD;1.2989" };
            Assert.Throws<ArgumentException>(() => new RequestProcessor(textEntry).CalculateBestRate());
        }

        [Fact]
        public void Initialise_NoPossibleRoute_ThrowsArgumentException()
        {
            var textEntry = new string[] { "EUR;550;PND", "6", "AUD;CHF;0.9661", "JPY;KRW;13.1151", "EUR;CHF;1.2053", "AUD;JPY;86.0305", "EUR;USD;1.2989", "JPY;INR;0.6571" };
            Assert.Throws<ArgumentException>(() => new RequestProcessor(textEntry).CalculateBestRate());
        }

        [Fact]
        public void Initialise_ShortRouteMoreExpensive_SuccessWithShortestRoute()
        {
            var textEntry = new string[] { "EUR;550;JPY", "8", "AUD;CHF;2.9661", "CHF;PND;1", "PND;AUD;1", "JPY;KRW;13.1151", "EUR;CHF;1.2053", "AUD;JPY;86.0305", "EUR;USD;1.2989", "JPY;INR;0.6571" };
            Assert.Equal(19225, new RequestProcessor(textEntry).CalculateBestRate());
        }

        [Fact]
        public void Initialise_MultipleRoutes_SuccessWithLeastExpensiveRoute()
        {
            var textEntry = new string[] { "EUR;550;JPY", "7", "AUD;CHF;2.9661", "CHF;AUD;1", "JPY;KRW;13.1151", "EUR;CHF;1.2053", "AUD;JPY;86.0305", "EUR;USD;1.2989", "JPY;INR;0.6571" };
            Assert.Equal(57031, new RequestProcessor(textEntry).CalculateBestRate());
        }
    }
}