﻿using Autofac;

namespace PlexRipper.BaseTests.Config
{
    public class BaseTestsAutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // builder.RegisterType<PlexMockServer>().As<IPlexMockServer>().SingleInstance();
        }
    }
}