import { parseSkylink } from "skynet-js";
import { convertSkylinkToBase32 } from "skynet-js/dist/utils";
import { useState } from "react";

const useBase32Subdomain = () => {
  const [base32SubdomainUrl, setBase32SubdomainUrl] = useState("");

  const base32SubdomainLookup = (skyfileUrl) => {
    let skylink = parseSkylink(skyfileUrl);
    let portalName = "";
    let protocol;

    if (skylink) {
      try {
        const url = new URL(skyfileUrl);
        // console.log(url);
        portalName = url.hostname;
        protocol = url.protocol;
      } catch (e) {
        // console.log("url error");
        portalName = "siasky.net";
        protocol = "https:";
      }

      let onlyPath = parseSkylink(skyfileUrl, { onlyPath: true });
      // console.log(portalName);

      setBase32SubdomainUrl(
        `${protocol}//${convertSkylinkToBase32(
          skylink
        )}.${portalName}${onlyPath}`
      );
    } else {
      setBase32SubdomainUrl("");
    }
  };

  return [base32SubdomainUrl, base32SubdomainLookup];
};

export default useBase32Subdomain;
