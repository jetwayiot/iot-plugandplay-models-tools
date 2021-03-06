﻿using System.CommandLine;
using System.IO;
using System.Text.Json;

namespace Azure.IoT.DeviceModelsRepository.CLI
{
    class CommonOptions
    {
        private const string _defaultRepository = "https://devicemodels.azure.com";

        public static Option<string> Dtmi
        {
            get
            {
                return new Option<string>(
                "--dtmi",
                description: "Digital Twin Model Identifier. Example: dtmi:com:example:Thermostat;1")
                {
                    Argument = new Argument<string>
                    {
                        Arity = ArgumentArity.ExactlyOne,
                    }
                };
            }
        }

        public static Option<string> Repo
        {
            get
            {
                return new Option<string>(
                  "--repository",
                  description: "Model Repository location. Can be remote endpoint or local directory.",
                  getDefaultValue: () => _defaultRepository
                  );
            }
        }

        public static Option<string> LocalRepo
        {
            get
            {
                return new Option<string>(
                  new string[] { "--local-repo", "--local-repository" },
                  description: "Local Model Repository path. " +
                  "If no path is provided the current working directory is used.",
                  getDefaultValue: () => null);
            }
        }

        public static Option<string> Output
        {
            get
            {
                return new Option<string>(
                    new string[] { "--output", "-o" },
                    description: "Desired file path to write result contents.",
                    getDefaultValue: () => null
                    );
            }
        }

        public static Option<FileInfo> ModelFile
        {
            get
            {
                return new Option<FileInfo>(
                    "--model-file",
                    description: "Path to file containing Digital Twins model content.")
                {
                    Argument = new Argument<FileInfo>().ExistingOnly()
                };
            }
        }

        public static Option<bool> Silent
        {
            get
            {
                return new Option<bool>(
                  "--silent",
                  description: "Silences command result output on stdout.",
                  getDefaultValue: () => false
                  );
            }
        }

        public static Option<bool> Strict
        {
            get{
                return new Option<bool>(
                    "--strict",
                    description: "Runs additional validation of file paths, DTMI scoping, and searches for reserved words.",
                    getDefaultValue: () => false
                );
            }
        }

        public static JsonDocumentOptions DefaultJsonParseOptions
        {
            get
            {
                return new JsonDocumentOptions
                {
                    AllowTrailingCommas = true,

                };
            }
        }

        public static JsonSerializerOptions DefaultJsonSerializerOptions
        {
            get
            {
                return new JsonSerializerOptions
                {
                    AllowTrailingCommas = true,
                    WriteIndented = true,
                };
            }
        }
    }
}
