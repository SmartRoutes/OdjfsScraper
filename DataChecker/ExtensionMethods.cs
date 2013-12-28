﻿using System;
using System.IO;
using ManyConsole;
using OdjfsScraper.DataChecker.Commands;

namespace OdjfsScraper.DataChecker
{
    public static class ExtensionMethods
    {
        public static string GetMapQuestKey(this Command command)
        {
            string mapQuestKey = Settings.MapQuestKey;
            if (string.IsNullOrWhiteSpace(mapQuestKey))
            {
                throw new ConsoleHelpAsException(string.Format("No MapQuest API key has been specified in {0}.",
                    Path.GetFileName(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile)));
            }

            return mapQuestKey;
        }
    }
}