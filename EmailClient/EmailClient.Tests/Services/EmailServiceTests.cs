using EmailClient.Core.DTOs;
using EmailClient.Core.Interfaces;
using EmailClient.Core.Requests;
using EmailClient.Services;
using NSubstitute;
using Xunit;

namespace EmailClient.Tests.Services
{
    public class EmailServiceTests
    {
        private readonly ISmtpClient _smtpClient;
        private readonly IImapClient _imapClient;
        private readonly EmailService _emailService;

        public EmailServiceTests()
        {
            _imapClient = Substitute.For<IImapClient>();
            _smtpClient = Substitute.For<ISmtpClient>();
            _emailService = new EmailService(_smtpClient, _imapClient);
        }

        [Fact]
        public async Task SendEmailAsync_WhenValidRequest_CallsSmtpClientOnce()
        {
            // Arrange
            var request = new SendEmailRequest
            {
                From = "sender@example.com",
                To = "receiver@example.com",
                Subject = "Test Subject",
                Body = "Test"
            };

            // Act
            await _emailService.SendEmailAsync(request);

            // Assert
            await _smtpClient.Received(1).SendEmailAsync(Arg.Is<EmailMessageDto>(msg =>
                msg.From == request.From &&
                msg.To == request.To &&
                msg.Subject == request.Subject &&
                msg.Body == request.Body
            ));
        }

        [Theory]
        [InlineData("invalid-email", "receiver@example.com")]
        [InlineData("sender@example.com", "invalid-email")]
        public async Task SendEmailAsync_WhenInvalidEmails_ThrowsInvalidDataException(string from, string to)
        {
            // Arrange
            var request = new SendEmailRequest
            {
                From = from,
                To = to,
                Subject = "Subject",
                Body = "Body"
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidDataException>(() => _emailService.SendEmailAsync(request));
            Assert.Equal("Invalid email address format", ex.Message);

            await _smtpClient.DidNotReceive().SendEmailAsync(Arg.Any<EmailMessageDto>());
        }

        [Fact]
        public async Task GetEmailsAsync_WhenValidRequest_ReturnsExpectedEmails()
        {
            // Arrange
            var request = new GetEmailsRequest();

            var expectedEmailSubject = "Test Subject 1";
            var expectedEmailBody = "Test Body 1";

            var expectedEmails = new List<EmailDto>
            {
                new EmailDto { From = "123@example.com", To = "789@example.com", Subject = expectedEmailSubject, Body = "Test Body 1", Date = DateTime.UtcNow },
                new EmailDto { From = "456@example.com", To = "91011@example.com", Subject = "Test 2", Body = expectedEmailBody, Date = DateTime.UtcNow }
            };

            _imapClient.ReadEmailsAsync(request).Returns(expectedEmails);

            // Act
            var result = await _emailService.GetEmailsAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Emails.Count);
            Assert.Equal(expectedEmailSubject, result.Emails[0].Subject);
            Assert.Equal(expectedEmailBody, result.Emails[1].Body);

            await _imapClient.Received(1).ReadEmailsAsync(request);
        }

        [Fact]
        public async Task GetEmailsAsync_WhenImapReturnsEmptyList_ReturnsEmptyResponse()
        {
            // Arrange
            var request = new GetEmailsRequest();
            _imapClient.ReadEmailsAsync(request).Returns(new List<EmailDto>());

            // Act
            var result = await _emailService.GetEmailsAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Emails);

            await _imapClient.Received(1).ReadEmailsAsync(request);
        }
    }
}
