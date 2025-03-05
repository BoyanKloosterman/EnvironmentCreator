using UnityEngine;

[System.Serializable]
public class User
{
    public string Email;
    public string Password;

    public User(string email, string password)
    {
        Email = email;
        Password = password;
    }
}
