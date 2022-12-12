namespace SkyDrop.Core.DataModels
{
    public class OnboardingContent
    {
        public static OnboardingContent[] Content => new[]
        {
            new OnboardingContent
            {
                Title = "SkyDrive",
                Description =
                    "- Recently sent and received files are saved in the SkyDrive for easy access\n- Create your own folders to organise your files\n- This is experimental tech so please keep backups of your files elsewhere",
                Icon = "ic_cloud"
            },
            new OnboardingContent
            {
                Title = "Portals",
                Description =
                    "- Choose your preferred Skynet portals and rank them in order of preference\n- Uploads will first be attempted on your first choice portal, if the upload fails then the upload will be retried using the next portal in the list",
                Icon = "ic_portals"
            },
            new OnboardingContent
            {
                Title = "End-to-end encryption",
                Description =
                    "- Send encrypted files to your contacts for additional data security\n- To add a new contact, pair with the contact's device by scanning their public key QR code\n- Encrypted files can only be decrypted by the specified recipient device",
                Icon = "ic_encryption"
            }
        };

        public string Title { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
    }
}