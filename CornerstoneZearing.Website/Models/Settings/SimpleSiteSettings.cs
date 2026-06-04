using System.ComponentModel.DataAnnotations;
using Cofoundry.Core.Configuration;

namespace CornerstoneZearing.Website;

/// <summary>
/// Cofoundry includes a system for easily creating strongly typed configuration 
/// settings just by defining a POCO class that inherits from IConfigurationSettings. 
/// These settings classes are automatically picked up by the DI system and bound 
/// to your config source (e.g. web.config/app.config) at runtime.
/// 
/// See https://www.cofoundry.org/docs/framework/configuration-settings
/// </summary>
public class WebsiteSettings : IConfigurationSettings
{
    /// <summary>
    /// Setting Name = CornerstoneZearing.Website:ContactRequestNotificationToAddress
    /// </summary>
    [Required]
    [EmailAddress]
    public string ContactRequestNotificationToAddress { get; set; } = string.Empty;
}
