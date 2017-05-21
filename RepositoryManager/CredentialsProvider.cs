namespace RepositoryManager
{
    using System;
    using CredentialManagement;
    using LibGit2Sharp;
    using Microsoft.Alm.Authentication;
    using Credential = CredentialManagement.Credential;

    internal static class CredentialsProvider
    {
        public static Credentials WinCred(string url, string usernameFromUrl, SupportedCredentialTypes types)
        {
            var creds = new Credential { Target = url, Type = CredentialType.Generic };
            if (creds.Load())
                return new UsernamePasswordCredentials { Username = creds.Username, Password = creds.Password };
            var uri = new Uri(url);
            var userPart = (string.IsNullOrEmpty(usernameFromUrl) ? string.Empty : usernameFromUrl + "@");
            creds.Target = string.Format("git:{0}://{1}{2}", uri.Scheme, userPart, uri.Host);

            if (!creds.Load())
            {
                creds.Target += "/";
                creds.Load();
            }

            return new UsernamePasswordCredentials { Username = creds.Username, Password = creds.Password };
        }

        public static Credentials MicrosoftAlm(string url, string usernameFromUrl, SupportedCredentialTypes types)
        {
            var cleanUrl = new Uri(url).GetComponents(UriComponents.SchemeAndServer, UriFormat.SafeUnescaped);
            var auth = new BasicAuthentication(new SecretStore("git"));
            var newCreds = auth.GetCredentials(new TargetUri(cleanUrl));
            return newCreds == null ? null
                : new UsernamePasswordCredentials
                {
                    Username = newCreds.Username,
                    Password = newCreds.Password
                };
        }
    }
}
