using LanguageExt;
using MongoDB.Driver;

namespace Servus.Akka.Telegram.TestBot.Repos;

public record TestInviteExtension(string Code, int Number);

public class TestInviteExtensionRepository
{
    private readonly IMongoCollection<TestInviteExtension> _extensionCollection;

    public TestInviteExtensionRepository(IMongoCollection<TestInviteExtension> extensionCollection)
    {
        _extensionCollection = extensionCollection;
    }

    public Option<TestInviteExtension> GetExtension(string code)
    {
        return _extensionCollection.FindSync(f => f.Code == code).FirstOrDefault();
    }

    public void Insert(string code, int number)
    {
        _extensionCollection.InsertOne(new TestInviteExtension(code, number));
    }
}