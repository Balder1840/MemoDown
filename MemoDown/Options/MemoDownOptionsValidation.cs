using Microsoft.Extensions.Options;

namespace MemoDown.Options
{
    public class MemoDownOptionsValidation : IValidateOptions<MemoDownOptions>
    {
        public MemoDownOptions _config { get; private set; }

        public MemoDownOptionsValidation(IConfiguration config)
        {
            _config = config.GetSection(MemoDownOptions.Key).Get<MemoDownOptions>()!;
        }

        public ValidateOptionsResult Validate(string? name, MemoDownOptions options)
        {
            List<string> errors = new List<string>();

            if (string.IsNullOrWhiteSpace(options.MemoDir))
            {
                errors.Add($"{nameof(MemoDownOptions.MemoDir)} is required.");
            }
            if (string.IsNullOrWhiteSpace(options.UploadsRelativePath))
            {
                errors.Add($"{nameof(MemoDownOptions.UploadsRelativePath)} is required.");
            }
            if (string.IsNullOrWhiteSpace(options.UploadsVirtualPath))
            {
                errors.Add($"{nameof(MemoDownOptions.UploadsVirtualPath)} is required.");
            }

            if (string.IsNullOrWhiteSpace(options.Account.UserName))
            {
                errors.Add($"{nameof(Account.UserName)} is required.");
            }
            if (string.IsNullOrWhiteSpace(options.Account.Password))
            {
                errors.Add($"{nameof(Account.Password)} is required.");
            }

            if (options.CloudflareTurnstile.Enable)
            {
                if (string.IsNullOrWhiteSpace(options.CloudflareTurnstile.SiteKey))
                {
                    errors.Add($"{nameof(CloudflareTurnstile.SiteKey)} is required.");
                }
                if (string.IsNullOrWhiteSpace(options.CloudflareTurnstile.SecretKey))
                {
                    errors.Add($"{nameof(CloudflareTurnstile.SecretKey)} is required.");
                }
            }

            if (options.Github.Enable)
            {
                if (string.IsNullOrWhiteSpace(options.Github.PAT))
                {
                    errors.Add($"{nameof(Github.PAT)} is required.");
                }
                if (string.IsNullOrWhiteSpace(options.Github.RepoName))
                {
                    errors.Add($"{nameof(Github.RepoName)} is required.");
                }
                if (string.IsNullOrWhiteSpace(options.Github.RepoOwner))
                {
                    errors.Add($"{nameof(Github.RepoOwner)} is required.");
                }
                if (string.IsNullOrWhiteSpace(options.Github.Branch))
                {
                    errors.Add($"{nameof(Github.Branch)} is required.");
                }

                if (options.Github.EnableAutoSync && string.IsNullOrWhiteSpace(options.Github.AutoSyncAt))
                {
                    errors.Add($"{nameof(Github.AutoSyncAt)} is required.");
                }
            }

            if (errors.Any())
            {
                return ValidateOptionsResult.Fail(errors);
            }

            return ValidateOptionsResult.Success;
        }
    }
}
