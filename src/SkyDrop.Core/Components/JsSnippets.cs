using System;
namespace SkyDrop.Core.Components
{
    public static class JsSnippets
    {
        public const string GetApiKey = @"
            function getElementByXpath(xpath)
            {
                return document.evaluate(xpath, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue;
            }

            function sleep(ms)
            {
                return new Promise(resolve => setTimeout(resolve, ms));
            }

            (async function()
            {
                await sleep(500);

                let myAccountButton = getElementByXpath(""//button[contains(text(),'My account')]"");
                console.log(myAccountButton);
                myAccountButton.click();

                await sleep(500);

                let settingsButton = getElementByXpath(""//span[contains(text(),'Settings')]"");
                console.log(settingsButton);
                settingsButton.click();

                await sleep(500);

                let developerSettingsButton = getElementByXpath(""//a[contains(text(),'Developer settings')]"");
                console.log(developerSettingsButton);
                developerSettingsButton.click();

                await sleep(500);

                let generateKeyButton = getElementByXpath(""//button[contains(text(),'Generate your API key')]"");
                console.log(generateKeyButton);
                generateKeyButton.click();

                await sleep(500);

                let keyContainer = getElementByXpath(""//code"");
                console.log(keyContainer);
                console.log(keyContainer.innerText);

                invokeCSharpAction({ apiKey: keyContainer.innerText });
            })()
            ";

        public const string CheckLoggedIn = @"
            function getElementByXpath(xpath)
            {
                return document.evaluate(xpath, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue;
            }

            (function()
            {
                myAccountButton = getElementByXpath(""//button[contains(text(),'My account')]"");
                console.log(myAccountButton);
                return myAccountButton != null;
            })()
            ";
    }
}