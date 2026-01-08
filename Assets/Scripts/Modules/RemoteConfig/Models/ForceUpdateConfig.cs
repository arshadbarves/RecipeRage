using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Modules.RemoteConfig.Models
{
    /// <summary>
    /// Configuration model for force update requirements
    /// </summary>
    [Serializable]
    public class ForceUpdateConfig : IConfigModel
    {
        [JsonProperty("platformRequirements")]
        public List<PlatformVersionRequirement> PlatformRequirements { get; set; }
        
        public ForceUpdateConfig()
        {
            PlatformRequirements = new List<PlatformVersionRequirement>();
        }
        
        public bool Validate()
        {
            if (PlatformRequirements == null || PlatformRequirements.Count == 0)
            {
                return false;
            }
            
            foreach (var requirement in PlatformRequirements)
            {
                if (string.IsNullOrEmpty(requirement.Platform))
                {
                    return false;
                }
                
                if (string.IsNullOrEmpty(requirement.MinimumVersion))
                {
                    return false;
                }
            }
            
            return true;
        }
        
        public PlatformVersionRequirement GetRequirementForPlatform(string platform)
        {
            return PlatformRequirements?.Find(r => 
                r.Platform.Equals(platform, StringComparison.OrdinalIgnoreCase));
        }
    }
    
    [Serializable]
    public class PlatformVersionRequirement
    {
        [JsonProperty("platform")]
        public string Platform { get; set; }
        
        [JsonProperty("minimumVersion")]
        public string MinimumVersion { get; set; }
        
        [JsonProperty("recommendedVersion")]
        public string RecommendedVersion { get; set; }
        
        [JsonProperty("updateUrgency")]
        public UpdateUrgency UpdateUrgency { get; set; }
        
        [JsonProperty("updateMessage")]
        public string UpdateMessage { get; set; }
        
        [JsonProperty("updateTitle")]
        public string UpdateTitle { get; set; }
        
        [JsonProperty("storeUrl")]
        public string StoreUrl { get; set; }
        
        public PlatformVersionRequirement()
        {
            UpdateUrgency = UpdateUrgency.Optional;
            UpdateTitle = "Update Available";
            UpdateMessage = "A new version is available. Please update to continue.";
        }
        
        public bool IsUpdateRequired(string currentVersion)
        {
            if (string.IsNullOrEmpty(currentVersion) || string.IsNullOrEmpty(MinimumVersion))
            {
                return false;
            }
            
            return CompareVersions(currentVersion, MinimumVersion) < 0;
        }
        
        public bool IsUpdateRecommended(string currentVersion)
        {
            if (string.IsNullOrEmpty(currentVersion) || string.IsNullOrEmpty(RecommendedVersion))
            {
                return false;
            }
            
            return CompareVersions(currentVersion, RecommendedVersion) < 0;
        }
        
        private int CompareVersions(string version1, string version2)
        {
            var v1Parts = version1.Split('.');
            var v2Parts = version2.Split('.');
            
            int maxLength = Math.Max(v1Parts.Length, v2Parts.Length);
            
            for (int i = 0; i < maxLength; i++)
            {
                int v1Part = i < v1Parts.Length && int.TryParse(v1Parts[i], out int p1) ? p1 : 0;
                int v2Part = i < v2Parts.Length && int.TryParse(v2Parts[i], out int p2) ? p2 : 0;
                
                if (v1Part < v2Part) return -1;
                if (v1Part > v2Part) return 1;
            }
            
            return 0;
        }
    }
    
    [Serializable]
    public enum UpdateUrgency
    {
        [JsonProperty("optional")]
        Optional,
        
        [JsonProperty("recommended")]
        Recommended,
        
        [JsonProperty("required")]
        Required
    }
}
