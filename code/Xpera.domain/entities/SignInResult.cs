namespace Xpera.domain.entities
{
    // What happens when someone signs in. An account locks after 3 wrong passwords.
    public enum SignInResult { Ok, WrongPassword, Locked }
}
