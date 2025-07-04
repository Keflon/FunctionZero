﻿using LocalizationZero.MarkupExtensions;

namespace ShowcaseZero.Localization
{
    /// <summary>
    /// To support multi-language and get intellisense in our markup, we can derive from 
    /// BaseLanguageExtension and give it an enum representing indeces of our strings.
    /// Then we simply do this:
    /// 
    ///   <Label Text="{tz:Lang TextId=E_Hello}"/>
    ///   
    ///   or more generally,
    ///   
    ///   <SomeElement SomeProperty="{tz:Lang TextId=E_Hello}"/>
    ///   
    /// </summary>
    public class LangExtension : BaseLocalizationExtension<LocalizationStrings>
    {
        public LangExtension() : base("languageResource")
        {

        }
    }


    /*
    
    This is how languages are set. 
    SetLanguage can be called willy-nilly at runtime from anywhere on the UI thread and *everything* updates *immediately*

    translationService.RegisterLanguage("English", new LanguageProvider(GetEnglish, "English"));
    translationService.RegisterLanguage("German", new LanguageProvider(GetGerman, "Deutsch"));

    // Setting to 'Application.Current.Resources' is app-wide, so is probably what you want to do.
    translationService.SetLanguage(Application.Current.Resources, "German");

    // This is an example of a 'language getter' - In reality we could e.g. load an embedded resource, er even make an API call!
    private string[] GetEnglish() => new string[] { "Hello", "World" };
    private string[] GetGerman() => new string[] { "Hallo", "Welt" };

    */
}
