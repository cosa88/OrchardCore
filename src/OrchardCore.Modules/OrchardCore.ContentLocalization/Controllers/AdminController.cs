using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Contents;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;

namespace OrchardCore.ContentLocalization.Controllers
{
    public class AdminController : Controller, IUpdateModel
    {
        private readonly IContentManager _contentManager;
        private readonly IContentLocalizationManager _contentLocalizationManager;
        private readonly INotifier _notifier;
        private readonly IAuthorizationService _authorizationService;
        private readonly IHtmlLocalizer<AdminController> H;

        public AdminController(
            IContentManager contentManager,
            INotifier notifier,
            IContentLocalizationManager localizationManager,
            IHtmlLocalizer<AdminController> localizer,
            IAuthorizationService authorizationService)
        {
            _contentManager = contentManager;
            _notifier = notifier;
            _authorizationService = authorizationService;
            _contentLocalizationManager = localizationManager;
            H = localizer;
        }

        [HttpPost]
        public async Task<IActionResult> Localize(string contentItemId, string targetCulture)
        {
            // Invariant culture name is empty so a null value is bound.
            targetCulture = targetCulture ?? "";

            var contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.Latest);

            if (contentItem == null)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.LocalizeContent, contentItem))
            {
                return Unauthorized();
            }

            var checkContentItem = await _contentManager.NewAsync(contentItem.ContentType);

            // Set the current user as the owner to check for ownership permissions on creation
            checkContentItem.Owner = User.Identity.Name;

            if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.EditContent, checkContentItem))
            {
                return Unauthorized();
            }

            var part = contentItem.As<LocalizationPart>();

            if (part == null)
            {
                return NotFound();
            }

            var alreadyLocalizedContent = await _contentLocalizationManager.GetContentItemAsync(part.LocalizationSet, targetCulture);

            if (alreadyLocalizedContent != null)
            {
                _notifier.Warning(H["A localization already exist for '{0}'", targetCulture]);
                return RedirectToAction("Edit", "Admin", new { area = "OrchardCore.Contents", contentItemId = contentItemId });
            }

            try
            {
                var newContent = await _contentLocalizationManager.LocalizeAsync(contentItem, targetCulture);
                _notifier.Information(H["Successfully created localized version of the content."]);
                return RedirectToAction("Edit", "Admin", new { area = "OrchardCore.Contents", contentItemId = newContent.ContentItemId });
            }
            catch (InvalidOperationException)
            {
                _notifier.Warning(H["Could not create localized version of the content item"]);
                return RedirectToAction("Edit", "Admin", new { area = "OrchardCore.Contents", contentItemId = contentItem.ContentItemId });
            }
        }
    }
}
