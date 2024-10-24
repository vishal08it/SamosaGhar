﻿namespace SamosaGhar.Config
{
    public class CloudinaryConfig
    {
        public string CloudName { get; }
        public string ApiKey { get; }
        public string ApiSecret { get; }

        public CloudinaryConfig(string cloudName, string apiKey, string apiSecret)
        {
            CloudName = cloudName;
            ApiKey = apiKey;
            ApiSecret = apiSecret;
        }
    }
}
