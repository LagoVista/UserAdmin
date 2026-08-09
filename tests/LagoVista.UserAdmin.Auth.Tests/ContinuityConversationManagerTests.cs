using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Managers;
using LagoVista.UserAdmin.Models.Users;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public class ContinuityConversationManagerTests
    {
        [Test]
        public async Task SendAsync_Should_Persist_Exchange_And_Return_Transcript()
        {
            var repo = new Mock<IContinuityConversationRepo>();
            var stored = new List<ContinuityConversationMessage>();
            repo.Setup(item => item.AppendAsync("actor-id", It.IsAny<IEnumerable<ContinuityConversationMessage>>())).Callback<string, IEnumerable<ContinuityConversationMessage>>((_, messages) => stored.AddRange(messages)).Returns(Task.CompletedTask);
            repo.Setup(item => item.GetAsync("actor-id")).ReturnsAsync(() => stored);
            var manager = new ContinuityConversationManager(repo.Object);

            var result = await manager.SendAsync("actor-id", "visitor", new ContinuityConversationMessageRequest { Message = "Hello, universe" });

            Assert.That(result.Successful, Is.True);
            Assert.That(result.Result.Messages.Count, Is.EqualTo(2));
            Assert.That(result.Result.Messages[0].Role, Is.EqualTo(ContinuityConversationRoles.User));
            Assert.That(result.Result.Messages[0].Text, Is.EqualTo("Hello, universe"));
            Assert.That(result.Result.Messages[1].Role, Is.EqualTo(ContinuityConversationRoles.Assistant));
            Assert.That(result.Result.Messages[1].Text, Is.Not.Empty);
        }

        [TestCase("create my workspace")]
        [TestCase("Please save my work now")]
        [TestCase("I'm ready!")]
        public async Task SendAsync_Should_Offer_Promotion_For_Visitor_Keywords(string message)
        {
            var repo = CreateRepo();
            var manager = new ContinuityConversationManager(repo.Object);

            var result = await manager.SendAsync("actor-id", "visitor", new ContinuityConversationMessageRequest { Message = message });

            Assert.That(result.Successful, Is.True);
            Assert.That(result.Result.Directive, Is.EqualTo(ContinuityConversationManager.OfferPromotionDirective));
        }

        [Test]
        public async Task SendAsync_Should_Not_Offer_Promotion_To_Provisional_Identity()
        {
            var manager = new ContinuityConversationManager(CreateRepo().Object);

            var result = await manager.SendAsync("actor-id", "provisional", new ContinuityConversationMessageRequest { Message = "I'm ready" });

            Assert.That(result.Successful, Is.True);
            Assert.That(result.Result.Directive, Is.Null);
        }

        [Test]
        public async Task SendAsync_Should_Reject_Oversized_Message()
        {
            var manager = new ContinuityConversationManager(CreateRepo().Object);

            var result = await manager.SendAsync("actor-id", "visitor", new ContinuityConversationMessageRequest { Message = new string('x', ContinuityConversationManager.MaximumMessageLength + 1) });

            Assert.That(result.Successful, Is.False);
        }

        [Test]
        public async Task ClearAsync_Should_Clear_Actor_Conversation()
        {
            var repo = CreateRepo();
            var manager = new ContinuityConversationManager(repo.Object);

            var result = await manager.ClearAsync("actor-id");

            Assert.That(result.Successful, Is.True);
            repo.Verify(item => item.ClearAsync("actor-id"), Times.Once);
        }

        private static Mock<IContinuityConversationRepo> CreateRepo()
        {
            var repo = new Mock<IContinuityConversationRepo>();
            repo.Setup(item => item.AppendAsync(It.IsAny<string>(), It.IsAny<IEnumerable<ContinuityConversationMessage>>())).Returns(Task.CompletedTask);
            repo.Setup(item => item.GetAsync(It.IsAny<string>())).ReturnsAsync(Enumerable.Empty<ContinuityConversationMessage>());
            repo.Setup(item => item.ClearAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
            return repo;
        }
    }
}
