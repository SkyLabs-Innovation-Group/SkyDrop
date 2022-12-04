using System;
namespace SkyDrop.Core.Components
{
    public static class JsSnippets
    {
        public const string GetApiKey = @"
            function GetElementByXpath(xpath)
            {
                return document.evaluate(xpath, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue;
            }

            (function()
            {
                myAccountButton = GetElementByXpath(""//button[contains(text(),'My account')]"");
                console.log(myAccountButton);
                myAccountButton.click();

                settingsButton = GetElementByXpath(""//span[contains(text(),'Settings')]"");
                console.log(settingsButton);
                settingsButton.click();

                developerSettingsButton = GetElementByXpath(""//a[contains(text(),'Developer settings')]"");
                console.log(developerSettingsButton);
                developerSettingsButton.click();

                generateKeyButton = GetElementByXpath(""//button[contains(text(),'Generate your API key')]"");
                console.log(generateKeyButton);
                generateKeyButton.click();

                keyContainer = GetElementByXpath(""//code"");
                console.log(keyContainer);
                console.log(keyContainer.innerText);

                return keyContainer.innerText;
            })()
            ";

        public const string CheckLoggedIn = @"
            function GetElementByXpath(xpath)
            {
                return document.evaluate(xpath, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue;
            }

            (function()
            {
                myAccountButton = GetElementByXpath(""//button[contains(text(),'My account')]"");
                console.log(myAccountButton);
                return myAccountButton != null;
            })()
            ";
    }
}