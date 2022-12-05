#undef XML_READY
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using CliWrap;
using T3.Core.Resource;
using T3.Operators.Utils.BmFont;

namespace Operators.Utils
{
    /// <summary>
    /// Uses msdf-atlas-gen to generate usable fonts from .tff and .otf files
    /// https://github.com/Chlumsky/msdf-atlas-gen
    /// Pre-compiled executable located in /Utilities
    ///
    /// Uses translated XML conversion logic from msdf-bmfont-xml's toBMFontXML
    /// https://github.com/soimy/msdf-bmfont-xml/blob/master/lib/utils.js
    ///
    /// Uses CLIWrap to invoke the executable.
    /// </summary>
    internal static class FontImporter
    {
        private static readonly HashSet<string> _supportedFileTypes = new()
         {
            ".ttf",
            ".otf"
         };

        private const string _fontPath = @$"{ResourceManager.ResourcesFolder}\fonts";
        private const string _cliOutputPath = @$"..\..\{_fontPath}";
        private const string _executablepath = @"Utilities\msdf-atlas-gen\msdf-atlas-gen.exe";
        private const string _generatedJsonSubfolder = ".generated-msdf-json";

        public static async Task<Font> TryImportFont(string path)
        {
            FileInfo fontFile = new FileInfo(path);
            if (!_supportedFileTypes.Contains(fontFile.Extension))
            {
                LogError($"File type not supported");
                return null;
            }
            
            
            string fontName = Path.GetFileNameWithoutExtension(fontFile.Name);
            string atlasOutPath = Path.Combine(_cliOutputPath, fontName + ".png");
            string jsonOutPath = Path.Combine(_cliOutputPath, _generatedJsonSubfolder, fontName + ".json");

            var msdfGenResult = await RunMSDFTool(fontFile, fontName, atlasOutPath, jsonOutPath, _executablepath);

            if (msdfGenResult == MSDFGenResult.Error)
            {
                LogError($"Error during MSDF generation. Check console for details.");
                return null;
            }
            
            MSDFJsonInfo jsonInfo;

            try
            {
                string json = File.ReadAllText(jsonOutPath);
                jsonInfo = JsonConvert.DeserializeObject<MSDFJsonInfo>(json);
            }
            catch (Exception e)
            {
                LogError("Error loading or parsing json: " + e);
                return null;
            }

            var hasNoKerningInfo = jsonInfo.kerning == null || jsonInfo.kerning.Count == 0;
            if (hasNoKerningInfo)
            {
                // is this an unrecoverable problem? do we just tell them to pick a different font or will it still work?
            }

            Font newFont = ToBMFont(jsonInfo);

            //save as XML

            return newFont;
        }

        private static async Task<MSDFGenResult> RunMSDFTool(FileInfo fontFile, string fontName, string atlasOutPath, string jsonOutPath, string executablePath)
        {
            Log($"Attempting MSDF generation of font {fontName} at {fontFile.FullName}.");
            var stdErrBuffer = new StringBuilder();

            await Cli.Wrap(executablePath)
                     .WithArguments(args => args
                                           .Add("-font").Add($"\"{fontFile.FullName}\"")
                                           .Add("type").Add("msdf")
                                           .Add("-format").Add("png")
                                           .Add("-dimensions").Add("1024 1024")
                                           .Add("-imageout").Add(atlasOutPath)
                                           .Add("-json").Add(jsonOutPath)
                                   )
                     .WithStandardOutputPipe(PipeTarget.ToDelegate(Log))
                     .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
                     .WithStandardErrorPipe(PipeTarget.ToDelegate(LogError))
                     .ExecuteAsync();

            if (stdErrBuffer.Length > 0)
            {
                // may want to parse buffer for specific errors later
                return MSDFGenResult.Error;
            }

            return MSDFGenResult.OK;
        }

        private static void Log(string message) =>  Console.WriteLine($"{LogPrefix}: {message}");
        private static void LogError(string message) => Console.Error.WriteLine($"{LogPrefix}: {message}");
        private const string LogPrefix = nameof(FontImporter);

        private enum MSDFGenResult { OK, Error }

        private static Font ToBMFont(MSDFJsonInfo data)
        {

#if XML_READY
            XmlConvert
            string xmlData = string.Empty;

            // Reorganize data structure
            // Definition: http://www.angelcode.com/products/bmfont/doc/file_format.html

            // info section
            xmlData.info = { };
            xmlData.info['@'] = data.info;
            xmlData.info['@'].padding = stringifyArray(data.info.padding, ',');
            xmlData.info['@'].spacing = stringifyArray(data.info.spacing, ',');
            // xmlData.info['@'].charset = stringifyArray(data.info.charset);
            xmlData.info['@'].charset = "";

            // common section
            xmlData.common = { };
            xmlData.common['@'] = data.common;

            // pages section, page shall be inserted later in module function callback
            xmlData.pages = { };
            xmlData.pages.page = [];
            data.pages.forEach((p, i) => {
                                   let page = { };
                                   page['@'] = { id: i, file: p};
                                   xmlData.pages.page.push(page);
                               });

            // distanceField section
            xmlData.distanceField = { };
            xmlData.distanceField['@'] = data.distanceField;

            // chars section
            xmlData.chars = { '@': { } };
            xmlData.chars['@'].count = data.chars.length;
            xmlData.chars.char = [];
            data.chars.forEach(c => {
                                   let char = { };
                                   char['@'] = c;
                                   xmlData.chars.char.push(char);
                               });

            // kernings section
            xmlData.kernings = { '@': { } };
            xmlData.kernings['@'].count = data.kernings.length;
            xmlData.kernings.kerning = [];
            data.kernings.forEach(k => {
                                      let kerning = { };
                                      kerning['@'] = k;
                                      xmlData.kernings.kerning.push(kerning);
                                  });

            return js2xmlparser.parse("font", xmlData, js2xmlOption);
#endif
            return new Font();
        }

        #region XML Classes

        #endregion XML Classes

        #region JSON Classes
        private class MSDFJsonInfo
        {
            public Atlas atlas;
            public Metrics metrics;
            public List<Glyphs> glyphs;
            public List<Kerning> kerning;
        }
        
        private class Atlas
        {
            public string type;
            public int distanceRange;
            public double size;
            public int width;
            public int height;
            public string yOrigin;
        }

        private class AtlasBounds
        {
            public double left;
            public double bottom;
            public double right;
            public double top;
        }

        private class Glyphs
        {
            public int unicode;
            public double advance;
            public PlaneBounds planeBounds;
            public AtlasBounds atlasBounds;
        }

        private class Metrics
        {
            public int emSize;
            public double lineHeight;
            public double ascender;
            public double descender;
            public double underlineY;
            public double underlineThickness;
        }

        private class PlaneBounds
        {
            public double left;
            public double bottom;
            public double right;
            public double top;
        }

        private class Kerning
        {
            public int unicode1;
            public int unicode2;
            public double advance;
        }
        #endregion JSON Classes
    }
}
