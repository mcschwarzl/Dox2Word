﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Dox2Word.Model;
using Dox2Word.Parser.Models;

namespace Dox2Word.Parser
{
    public class XmlParser
    {
        private readonly string basePath;

        public XmlParser(string basePath)
        {
            this.basePath = basePath;
        }

        public Project Parse()
        {
            string indexFile = Path.Combine(this.basePath, "index.xml");
            var index = Parse<DoxygenIndex>(indexFile);

            var project = new Project();

            // Discover the root groups
            var groupCompoundDefs = index.Compounds.Where(x => x.Kind == CompoundKind.Group)
                .ToDictionary(x => x.RefId, x => this.ParseDoxygenFile(x.RefId));
            var rootGroups = groupCompoundDefs.Keys.ToHashSet();
            foreach (var group in groupCompoundDefs.Values.ToList())
            {
                foreach (var innerGroup in group.InnerGroups)
                {
                    rootGroups.Remove(innerGroup.RefId);
                }
            }

            project.Groups.AddRange(rootGroups.Select(x => this.ParseGroup(groupCompoundDefs, x)).OrderBy(x => x.Name));

            return project;
        }

        private Group ParseGroup(Dictionary<string, CompoundDef> groups, string refId)
        {
            var compoundDef = groups[refId];

            var group = new Group()
            {
                Name = compoundDef.Title,
                Descriptions = ParseDescriptions(compoundDef),
            };
            group.SubGroups.AddRange(compoundDef.InnerGroups.Select(x => this.ParseGroup(groups, x.RefId)));
            group.Files.AddRange(compoundDef.InnerFiles.Select(x => x.Name));
            group.Classes.AddRange(compoundDef.InnerClasses.Select(x => this.ParseClass(x.RefId)));

            var members = compoundDef.Sections.SelectMany(x => x.Members);
            foreach (var member in members)
            {
                if (member.Kind == DoxMemberKind.Function)
                {
                    var function = new Function()
                    {
                        Name = member.Name,
                        Descriptions = ParseDescriptions(member),
                        ReturnType = LinkedTextToString(member.Type) ?? "",
                        ReturnDescription = ParseReturnDescription(member),
                        Definition = member.Definition ?? "",
                        ArgsString = member.ArgsString ?? "",
                    };
                    function.Parameters.AddRange(ParseParameters(member));
                    group.Functions.Add(function);
                }
                else if (member.Kind == DoxMemberKind.Define)
                {
                    var macro = new Macro()
                    { 
                        Name = member.Name,
                        Descriptions = ParseDescriptions(member),
                        ReturnDescription = ParseReturnDescription(member),
                        Initializer = LinkedTextToString(member.Initializer) ?? "",
                    };
                    macro.Parameters.AddRange(ParseParameters(member));
                    group.Macros.Add(macro);
                }
                else if (member.Kind == DoxMemberKind.Typedef)
                {
                    var typedef = new Typedef()
                    {
                        Name = member.Name,
                        Type = LinkedTextToString(member.Type) ?? "",
                        Definition = member.Definition ?? "",
                        Descriptions = ParseDescriptions(member),
                    };
                    group.Typedefs.Add(typedef);
                }
                else if (member.Kind == DoxMemberKind.Variable)
                {
                    group.GlobalVariables.Add(ParseVariable(member));
                }
            }

            return group;
        }

        private static IEnumerable<Parameter> ParseParameters(MemberDef member)
        {
            foreach (var param in member.Params)
            {
                if (param.Type?.Type is { Count: 1 } l && l[0] as string == "void")
                    continue;

                string name = param.DeclName ?? param.DefName ?? "";

                // Find its docs...
                var descriptionPara = member.DetailedDescription?.Para.SelectMany(x => x.ParameterLists)
                    .Where(x => x.Kind == DoxParamListKind.Param)
                    .SelectMany(x => x.ParameterItems)
                    .FirstOrDefault(x => x.ParameterNameList.Select(x => x.ParameterName).Contains(name))
                    ?.ParameterDescription.Para.FirstOrDefault();

                var functionParameter = new Parameter()
                {
                    Name = name,
                    Type = LinkedTextToString(param.Type),
                    Description = ParaToParagraph(descriptionPara),
                };

                yield return functionParameter;
            }
        }

        private Class ParseClass(string refId)
        {
            var compoundDef = this.ParseDoxygenFile(refId);

            if (compoundDef.Kind != CompoundKind.Struct)
                throw new ParserException($"Don't konw how to parse class kind {compoundDef.Kind} in {refId}");

            var cls = new Class()
            {
                Name = compoundDef.CompoundName ?? "",
                Descriptions = ParseDescriptions(compoundDef),
            };

            var members = compoundDef.Sections.SelectMany(x => x.Members)
                .Where(x => x.Kind == DoxMemberKind.Variable);
            foreach (var member in members)
            {
                cls.Variables.Add(ParseVariable(member));
            }

            return cls;
        }

        private static Variable ParseVariable(MemberDef member)
        {
            var variable = new Variable()
            {
                Name = member.Name ?? "",
                Type = LinkedTextToString(member.Type) ?? "",
                Definition = member.Definition ?? "",
                Descriptions = ParseDescriptions(member),
            };
            return variable;
        }

        private static IParagraph ParseReturnDescription(MemberDef member)
        {
            return ParaToParagraph(member.DetailedDescription?.Para.SelectMany(x => x.Parts)
                .OfType<DocSimpleSect>()
                .FirstOrDefault(x => x.Kind == DoxSimpleSectKind.Return)?.Para);
        }

        private static string? LinkedTextToString(LinkedText? linkedText)
        {
            if (linkedText == null)
                return null;

            return string.Join(" ", linkedText.Type.Select(x =>
                x switch
                {
                    string s => s,
                    RefText r => r.Name,
                    _ => throw new ParserException($"Unknown element in LinkedText: {x}"),
                }));
        }

        private static Descriptions ParseDescriptions(IDoxDescribable member)
        {
            var descriptions = new Descriptions()
            {
                BriefDescription = ParasToParagraph(member.BriefDescription?.Para),
            };
            descriptions.DetailedDescription.AddRange(ParasToParagraphs(member.DetailedDescription?.Para));
            return descriptions;
        }

        private static IParagraph ParaToParagraph(DocPara? para)
        {
            return ParaToParagraphs(para).FirstOrDefault() ?? new TextParagraph();
        }

        private static IParagraph ParasToParagraph(IEnumerable<DocPara>? paras)
        {
            return ParasToParagraphs(paras).FirstOrDefault() ?? new TextParagraph();
        }

        private static IEnumerable<IParagraph> ParasToParagraphs(IEnumerable<DocPara>? paras)
        {
            if (paras == null)
                return Enumerable.Empty<TextParagraph>();

            return paras.SelectMany(x => ParaToParagraphs(x)).Where(x => x.Count > 0);
        }

        private static List<IParagraph> ParaToParagraphs(DocPara? para)
        {
            var paragraphs = new List<IParagraph>();

            if (para == null)
                return paragraphs;

            Parse(paragraphs, para, TextRunFormat.None);

            static void Parse(List<IParagraph> paragraphs, DocPara? para, TextRunFormat format)
            {
                void NewParagraph(ParagraphType type = ParagraphType.Normal) => paragraphs.Add(new TextParagraph(type));

                void Add(TextRun textRun)
                {
                    if (paragraphs.LastOrDefault() is not TextParagraph paragraph)
                    {
                        paragraph = new TextParagraph();
                        paragraphs.Add(paragraph);
                    }
                    paragraph.Add(textRun);
                }
                void AddTextRun(string text, TextRunFormat format) => Add(new TextRun(text.TrimStart('\n'), format));

                if (para == null)
                    return;

                foreach (object? part in para.Parts)
                {
                    switch (part)
                    {
                        case string s:
                            AddTextRun(s, format);
                            break;
                        case DocSimpleSect s when s.Kind == DoxSimpleSectKind.Warning:
                            NewParagraph(ParagraphType.Warning);
                            Parse(paragraphs, s.Para, format);
                            NewParagraph();
                            break;
                        case OrderedList o:
                            ParseList(o, ListParagraphType.Number, format);
                            break;
                        case UnorderedList u:
                            ParseList(u, ListParagraphType.Bullet, format);
                            break;
                        case Listing l:
                            ParseListing(l);
                            break;
                        case BoldMarkup b:
                            Parse(paragraphs, b, format | TextRunFormat.Bold);
                            break;
                        case ItalicMarkup i:
                            Parse(paragraphs, i, format | TextRunFormat.Italic);
                            break;
                        case MonospaceMarkup m:
                            Parse(paragraphs, m, format | TextRunFormat.Monospace);
                            break;
                        case XmlElement e:
                            AddTextRun(e.InnerText, format);
                            break;
                        case DocSimpleSect:
                            break; // Ignore
                        default:
                            throw new ParserException($"Unexpected text {part} ({part.GetType()})");
                    };
                }

                void ParseList(DocList docList, ListParagraphType type, TextRunFormat format)
                {
                    var list = new ListParagraph(type);
                    paragraphs.Add(list);
                    foreach (var item in docList.Items)
                    {
                        // It could be that the para contains a warning or something which will add
                        // another paragraph. In this case, we'll just ignore it.
                        var paragraphList = new List<IParagraph>();
                        foreach (var para in item.Paras)
                        {
                            Parse(paragraphList, para, format);
                        }
                        list.Items.AddRange(paragraphList);
                    }
                }

                void ParseListing(Listing listing)
                {
                    var codeParagraph = new CodeParagraph();
                    paragraphs.Add(codeParagraph);
                    foreach (var codeline in listing.Codelines)
                    {
                        var sb = new StringBuilder();
                        foreach (var highlight in codeline.Highlights)
                        {
                            foreach (object part in highlight.Parts)
                            {
                                switch (part)
                                {
                                    case string s:
                                        sb.Append(s);
                                        break;
                                    case Sp:
                                        sb.Append(" ");
                                        break;
                                    case XmlElement e:
                                        sb.Append(e.InnerText);
                                        break;
                                    default:
                                        throw new ParserException($"Unexpected code part {part} ({part.GetType()})");
                                }
                            }
                        }
                        codeParagraph.Lines.Add(sb.ToString());
                    }
                }
            }

            return paragraphs;
        }

        private CompoundDef ParseDoxygenFile(string refId)
        {
            string filePath = Path.Combine(this.basePath, refId + ".xml");
            var file = Parse<DoxygenFile>(filePath);
            if (file.CompoundDefs.Count != 1)
                throw new ParserException($"File {filePath}: expected 1 compoundDef, got {file.CompoundDefs.Count}");
            return file.CompoundDefs[0];
        }

        private static class SerializerCache<T>
        {
            public static readonly XmlSerializer Instance = new XmlSerializer(typeof(T));
        }
        private static T Parse<T>(string filePath)
        {
            using (var stream = File.OpenRead(filePath))
            {
                return (T)SerializerCache<T>.Instance.Deserialize(stream)!;
            }
        }
    }
}
