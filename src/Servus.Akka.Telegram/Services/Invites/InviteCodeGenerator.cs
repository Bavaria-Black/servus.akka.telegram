namespace Servus.Akka.Telegram.Services;

internal static class InviteCodeGenerator
{
    private const int Base = 36;
    private const string Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    internal static string CreateInviteCode()
    {
        var guid = Guid.NewGuid().ToByteArray();
        var value1 = BitConverter.ToInt32(guid.Take(4).ToArray());
        var value2 = BitConverter.ToInt32(guid.Skip(4).Take(4).ToArray());
        var value3 = BitConverter.ToInt32(guid.Skip(8).Take(4).ToArray());
        var value4 = BitConverter.ToInt32(guid.Skip(12).Take(4).ToArray());

        var code  = CreateInviteCode(value1);
        code += CreateInviteCode(value2);
        code += CreateInviteCode(value3);
        code += CreateInviteCode(value4);

        return code;
    }

    private static string CreateInviteCode(Int32 value)
    {
        string result = "";
        while (value > 0)
        {
            result = Chars[value % Base] + result; // use StringBuilder for better performance
            value /= Base;
        }

        return result;
    }
}