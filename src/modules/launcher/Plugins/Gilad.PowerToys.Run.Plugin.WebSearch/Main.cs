// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using ManagedCommon;
using RestSharp;
using Wox.Infrastructure;
using Wox.Infrastructure.Storage;
using Wox.Plugin;
using Wox.Plugin.Logger;

namespace Gilad.PowerToys.Run.Plugin.WebSearch
{
    public class Main : IPlugin, IPluginI18n, IContextMenu, ISavable, IDisposable
    {
        private readonly PluginJsonStorage<UriSettings> _storage;
        private bool _disposed;
        private UriSettings _uriSettings;
        private RegistryWrapper _registryWrapper;

        public Main()
        {
            _storage = new PluginJsonStorage<UriSettings>();
            _uriSettings = _storage.Load();
            _registryWrapper = new RegistryWrapper();
        }

        public string DuckIconPath { get; set; }

        public string BrowserPath { get; set; }

        public string DefaultIconPath { get; set; }

        public PluginInitContext Context { get; protected set; }

        public string Name => Properties.Resources.plugin_name;

        public string Description => Properties.Resources.plugin_description;

        public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
        {
            return new List<ContextMenuResult>(0);
        }

        public List<Result> Query(Query query)
        {
            var results = new List<Result>();

            if (IsActivationKeyword(query)
                && IsDefaultBrowserSet())
            {
                var client = new RestClient("https://contextualwebsearch-websearch-v1.p.rapidapi.com");

                var request = new RestRequest("/api/Search/WebSearchAPI", Method.GET);

                // ?q=taylor%20swift&pageNumber=1&pageSize=10&autoCorrect=true
                request.AddQueryParameter("q", query.ToString());
                request.AddQueryParameter("pageNumber", "1");
                request.AddQueryParameter("pageSize", "2");
                request.AddQueryParameter("autoCorrect", "true");

                request.AddHeader("x-rapidapi-host", "contextualwebsearch-websearch-v1.p.rapidapi.com");
                request.AddHeader("x-rapidapi-key", "93a4832760mshba88be3fd6863e5p1094afjsn1a4bf1737aca");

                IRestResponse response = client.Execute(request);

                // results.Add(new Result {});
                return results;
            }

            /*
            if (!string.IsNullOrEmpty(query?.Search))
            {
                var uriResultString = uriResult.ToString();
                results.Add(new Result
                {
                    Title = query.ToString(),
                    SubTitle = Properties.Resources.plugin_action,
                    IcoPath = _uriSettings.ShowBrowserIcon
                        ? DuckIconPath
                        : DefaultIconPath,
                    Action = action =>
                    {
                        if (!Helper.OpenInShell(uriResultString))
                        {
                            var title = $"Plugin: {Properties.Resources.plugin_action}";
                            var message = $"{Properties.Resources.plugin_failed}: {uriResultString}";
                            Context.API.ShowMsg(title, message);
                            return false;
                        }

                        return true;
                    },
                });
            }
            */
            return results;
        }

        private static bool IsActivationKeyword(Query query)
        {
            return !string.IsNullOrEmpty(query?.ActionKeyword)
                            && query?.ActionKeyword == query?.RawQuery;
        }

        private bool IsDefaultBrowserSet()
        {
            return !string.IsNullOrEmpty(BrowserPath);
        }

        public void Init(PluginInitContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            Context.API.ThemeChanged += OnThemeChanged;
            UpdateIconPath(Context.API.GetCurrentTheme());
            UpdateBrowserIconPath(Context.API.GetCurrentTheme());
        }

        public string GetTranslatedPluginTitle()
        {
            return Properties.Resources.plugin_name;
        }

        public string GetTranslatedPluginDescription()
        {
            return Properties.Resources.plugin_description;
        }

        public void Save()
        {
            _storage.Save();
        }

        private void OnThemeChanged(Theme oldtheme, Theme newTheme)
        {
            UpdateIconPath(newTheme);
            UpdateBrowserIconPath(newTheme);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "We want to keep the process alive but will log the exception")]
        private void UpdateBrowserIconPath(Theme newTheme)
        {
            try
            {
                if (newTheme == Theme.Light || newTheme == Theme.HighContrastWhite)
                {
                    DuckIconPath = "Images/duck.light.png";
                }
                else
                {
                    DuckIconPath = "Images/duck.dark.png";
                }
            }
            catch (Exception e)
            {
                DuckIconPath = DefaultIconPath;
                Log.Exception("Exception when retrieving icon", e, GetType());
            }
        }

        private void UpdateIconPath(Theme theme)
        {
            if (theme == Theme.Light || theme == Theme.HighContrastWhite)
            {
                DefaultIconPath = "Images/duck.light.png";
            }
            else
            {
                DefaultIconPath = "Images/duck.dark.png";
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                if (Context != null && Context.API != null)
                {
                    Context.API.ThemeChanged -= OnThemeChanged;
                }

                _disposed = true;
            }
        }
    }
}
