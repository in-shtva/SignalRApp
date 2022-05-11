using NLog;
using Novell.Directory.Ldap;
using SignalRApp.Entities;
using SignalRApp.Models;
using System;
using Logger = NLog.Logger;

namespace SignalRApp.Services
{
    public static class LdapService
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public static ResultModel<UserEntity> GetLdapUser(string login, string pass)
        {
            var errMessage = "";
            try
            {
                var novelLdap = new LdapConnection();

                novelLdap.Connect(Settings.LdapHost, Settings.LdapPort);
                novelLdap.Bind(LdapConnection.LdapV3, $"uid={login},DC=nordclan", pass);

                var result = novelLdap.Search($"uid={login},DC=nordclan", LdapConnection.ScopeBase, "(objectClass=*)",
                    null, false);

                var ldapUser = result.Next();
                if (ldapUser == null)
                {
                    errMessage = $"LdapUser with userName {login} and password {pass} wasn't found";
                    _logger.Error(errMessage);
                    return new ResultModel<UserEntity>(errMessage);
                }

                var attrs = ldapUser.GetAttributeSet();
                var firstName = attrs["firstName"]?.StringValue ?? "";
                var lastName = attrs["lastName"]?.StringValue ?? "";
                var jpegPhoto = attrs["jpegPhoto"]?.StringValue ?? "";
                var email = attrs["emailPrimary"]?.StringValue ?? "";

                var usr = new UserEntity(firstName, lastName, login, email, jpegPhoto);
                return new ResultModel<UserEntity>(usr);
            }
            catch (Exception ex)
            {
                errMessage = $"Error: {ex.Message}";
                _logger.Error(errMessage);

                return new ResultModel<UserEntity>(errMessage);
            }
        }
    }
}
