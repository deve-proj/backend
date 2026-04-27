using System.Text;

public class Test
{
    public static void Main()
    {

        string text = "Hello world!";

        Console.WriteLine(Encoding.UTF8.GetBytes(text));

    }
}