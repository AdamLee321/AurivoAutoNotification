using System;
using System.Security;
using Microsoft.Exchange.WebServices.Data;

namespace EWServices
{
  public interface IUserData
  {
    ExchangeVersion Version { get; }
    string EmailAddress { get; }
    SecureString Password { get; }
    Uri AutodiscoverUrl { get; set; }
  }

  public class clsUserData : IUserData
  {
    public static clsUserData UserData;

    public static IUserData GetUserData(string username, string userpassword)
    {
      if (UserData == null)
      {
        SetUserData(username, userpassword);
      }

      return UserData;
    }

    private static void SetUserData(string usr, string pwd)
    {
            UserData = new clsUserData();

            UserData.EmailAddress = usr;
            UserData.Password = new SecureString();
            for (Int16 i = 0; i < pwd.Length; i++)
                UserData.Password.AppendChar(Convert.ToChar(pwd.Substring(i,1)));
            UserData.Password.MakeReadOnly();
        }

    private static void GetUserDataFromConsole()
    {
      UserData = new clsUserData();

      Console.Write("Enter email address: ");
      UserData.EmailAddress = Console.ReadLine();

      UserData.Password = new SecureString();

      Console.Write("Enter password: ");

      while (true)
      {
          ConsoleKeyInfo userInput = Console.ReadKey(true);
          if (userInput.Key == ConsoleKey.Enter)
          {
              break;
          }
          else if (userInput.Key == ConsoleKey.Escape)
          {
              return;
          }
          else if (userInput.Key == ConsoleKey.Backspace)
          {
              if (UserData.Password.Length != 0)
              {
                  UserData.Password.RemoveAt(UserData.Password.Length - 1);
              }
          }
          else
          {
              UserData.Password.AppendChar(userInput.KeyChar);
              Console.Write("*");
          }
      }

      Console.WriteLine();

      UserData.Password.MakeReadOnly();
    }

    public ExchangeVersion Version { get { return ExchangeVersion.Exchange2013; } }

    public string EmailAddress
    {
        get;
        private set;
    }

    public SecureString Password
    {
        get;
        private set;
    }

    public Uri AutodiscoverUrl
    {
        get;
        set;
    }
  }
}
