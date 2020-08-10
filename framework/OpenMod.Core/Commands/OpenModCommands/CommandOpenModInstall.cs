﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OpenMod.API.Commands;
using OpenMod.API.Localization;
using OpenMod.Core.Plugins.NuGet;
using OpenMod.NuGet;

namespace OpenMod.Core.Commands.OpenModCommands
{
    [Command("install")]
    [CommandAlias("i")]
    [CommandDescription("Install plugins from NuGet")]
    [CommandSyntax("<id[:version]> [-Pre]")]
    [CommandParent(typeof(CommandOpenMod))]
    public class CommandOpenModInstall : Command
    {
        private readonly IOpenModStringLocalizer m_StringLocalizer;
        private readonly IConfiguration m_Configuration;
        private readonly NuGetPluginAssembliesSource m_NuGetPlugins;

        public CommandOpenModInstall(
            IOpenModStringLocalizer stringLocalizer,
            IServiceProvider serviceProvider, 
            IConfiguration configuration,
            NuGetPluginAssembliesSource nuGetPlugins) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_Configuration = configuration;
            m_NuGetPlugins = nuGetPlugins;
        }

        protected override async Task OnExecuteAsync()
        {
            if(Context.Parameters.Count == 0)
            {
                throw new CommandWrongUsageException(Context);
            }

            var allowedActors = m_Configuration.GetSection("nuget:install:allowedActors").Get<string[]>();
            if (allowedActors.All(d => d.Trim() != "*" && !Context.Actor.Type.Equals(d.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                throw new UserFriendlyException(m_StringLocalizer["commands:openmod:restricted"]);
            }

            var args = Context.Parameters.ToList();
            var isPre = false;
            if (args.Contains("-Pre"))
            {
                isPre = true;
                args.Remove("-Pre");
            }

            foreach (var arg in args)
            {
                var packageInfo = arg.Split(':');
                var packageName = packageInfo[0];
                var packageVersion = packageInfo.Length > 1 ? packageInfo[1] : null;

                await Context.Actor.PrintMessageAsync($"Installing {arg}...", Color.White);
                var result = await m_NuGetPlugins.InstallPackageAsync(packageName, packageVersion, isPre);
                if (result.Code != NuGetInstallCode.Success)
                {
                    await Context.Actor.PrintMessageAsync($"Failed to install \"{packageName}\": " + result.Code, Color.DarkRed);
                    continue;
                }

                await Context.Actor.PrintMessageAsync($"Successfully installed {result.Identity.Id} v{result.Identity.Version}.", Color.DarkGreen);
            }

            await Context.Actor.PrintMessageAsync("To complete installation, please reload OpenMod with /openmod reload.", Color.White);
        }
    }
}