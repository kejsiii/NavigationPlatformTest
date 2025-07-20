namespace Application.Resources
{
    public static class StringResourceMessage
    {
        #region User
        public const string UserAlreadyExists = "User already exists.";
        public const string InvalidCredentials = "Invalid Credentials.";
        public const string UserNotFound = "User not found.";
        #endregion

        #region Role
        public const string RoleNotFound = "Role not found.";
        #endregion

        #region Journey
        public const string JourneyNotFound = "Journey not found.";
        public const string JourneyAlreadyExists = "Journey already exists";
        #endregion

        #region PublicLink
        public const string PublicLinkNotFound = "Public link not found";
        public const string PublicLinkRevoked = "This public link has been revoked.";
        #endregion
        public static string FormatErrorMessage(string error, object[] formatParameters = null, object[] parameters = null)
        {
            if (formatParameters != null && formatParameters.Length > 0)
            {
                error = string.Format(error, formatParameters);
            }

            if (parameters != null && parameters.Length > 0)
            {
                error += string.Join(CommonStringResources.Separator, parameters);
            }

            return error;
        }
    }
}
