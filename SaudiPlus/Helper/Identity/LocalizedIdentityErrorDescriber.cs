using Microsoft.AspNetCore.Identity;
using Resource = SaudiPlus.Resources.Identity.Resources;

namespace SaudiPlus.Helper.Identity
{
    public class LocalizedIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError DefaultError()
        {
            return new IdentityError
            {
                Code = nameof(DefaultError),
                Description = Resource.DefaultError
            };
        }

        /// <summary>
        /// Returns an <see cref="IdentityError"/> indicating a password mismatch.
        /// </summary>
        /// <returns>An <see cref="IdentityError"/> indicating a password mismatch.</returns>
        public override IdentityError PasswordMismatch()
        {
            return new IdentityError
            {
                Code = nameof(PasswordMismatch),
                Description = Resource.PasswordMismatch
            };
        }
    }
}
