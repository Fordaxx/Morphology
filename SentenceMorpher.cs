using System;
using System.Collections.Generic;
using System.Linq;

namespace Morphology
{
    public class SentenceMorpher
    {
        private readonly Dictionary<string, List<string>> dictionary;
        public SentenceMorpher(Dictionary<string, List<string>> _dictionary)
        {
            dictionary = _dictionary;
        }
        public static SentenceMorpher Create(IEnumerable<string> dictionaryLines)
        {
            var dictionary = new Dictionary<string, List<string>>();

            var isPreviousLineSpace = true;
            var lastActiveString = "";
            foreach (var line in dictionaryLines)
            {
                if (String.IsNullOrEmpty(line.Trim(' ')))
                    isPreviousLineSpace = true;
                else
                {
                    if (isPreviousLineSpace)
                    {
                        var isNumber = long.TryParse(line, out long tryNumber);
                        if (isNumber)
                            continue;
                        char[] separators = { ' ', ',', '\t' };
                        var words = line.Split(separators);
                        lastActiveString = words[0];
                        if (!dictionary.ContainsKey(words[0]))
                            dictionary.Add(words[0], new List<string> { line });
                        else
                            dictionary[lastActiveString].Add(line);
                        isPreviousLineSpace = false;
                    }
                    else
                        dictionary[lastActiveString].Add(line);
                }
            }
            return new SentenceMorpher(dictionary);
        }
        public virtual string Morph(string sentence)
        {
            if (string.IsNullOrEmpty(sentence.Trim(' ')))
                return string.Empty;
            var words = sentence.Split(' ', '\n');
            var result = new List<string>();
            foreach (var word in words)
            {
                if (word.Contains('{'))
                {
                    var normalisedWord = word.Substring(0, word.IndexOf('{'));
                    var attributes = word.Substring(word.IndexOf('{') + 1,
                                word.IndexOf('}') - word.IndexOf('{') - 1);
                    char[] separatiors = { ',', ' ' };

                    var attributesAsList = attributes.Split(separatiors).ToList();

                    if (string.IsNullOrEmpty(attributes.Trim(' ')))
                    {
                        result.Add(normalisedWord);
                        break;
                    }

                    if (dictionary.ContainsKey(normalisedWord.ToUpper()))
                    {
                        string maxIncludes = "";
                        var maxCounter = 0;
                        var AllLines = "";
                        foreach (var listLine in dictionary[normalisedWord.ToUpper()])
                        {
                            AllLines += listLine + " ";
                            var attributeCounter = 0;
                            foreach (var attribute in attributesAsList)
                            {
                                if (listLine.ToUpper().Contains(attribute.ToUpper()))
                                    attributeCounter++;
                                else
                                    break;
                            }
                            if (attributeCounter > maxCounter)
                            {
                                maxIncludes = listLine;
                                maxCounter = attributeCounter;
                            }
                        }
                        if (String.IsNullOrEmpty(maxIncludes))
                            result.Add(normalisedWord);
                        else
                            result.Add(maxIncludes.Substring(0, maxIncludes.IndexOf('\t')));
                    }
                }
                else
                    result.Add(word);
            }
            sentence = "";
            foreach (var item in result)
            {
                sentence += item + " ";
            }
            if (!string.IsNullOrEmpty(sentence))
                sentence = sentence.Remove(sentence.Length - 1);
            return sentence;
        }
    }
}