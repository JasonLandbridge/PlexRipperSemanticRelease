﻿using AutoMapper;
using PlexRipper.Application;
using PlexRipper.Data;
using PlexRipper.Domain.Config.AutoMapper;
using PlexRipper.PlexApi.Mappings;
using PlexRipper.Settings.Config;

namespace PlexRipper.WebAPI;

public static class MapperSetup
{
    public static MapperConfiguration Configuration => new(cfg =>
    {
        // Application
        cfg.AddProfile(new DomainMappingProfile());
        cfg.AddProfile(new ApplicationMappingProfile());

        // Infrastructure
        cfg.AddProfile(new PlexApiMappingProfile());
        cfg.AddProfile(new SettingsMappingProfile());
        cfg.AddProfile(new DataMappingProfile());

        // Presentation
        cfg.AddProfile(new WebApiMappingProfile());
    });

    public static IMapper CreateMapper()
    {
        return Configuration.CreateMapper();
    }
}