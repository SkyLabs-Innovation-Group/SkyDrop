import React, { useState, useEffect } from 'react';
import useDebounce from '../hooks/useDebounce';
import useBase32Subdomain from '../hooks/useBase32Subdomain';
import { Input, Typography, Divider } from 'antd';

import './PortalAlternatives.css';
// import { parseSkylink } from "skynet-js";
// import { convertSkylinkToBase32 } from "skynet-js/dist/utils";

const { Title, Link } = Typography;

const baseStyle = {
  flex: 1,
  display: 'flex',
  flexDirection: 'column',
  alignItems: 'center',
  padding: '20px',
  borderWidth: 2,
  borderRadius: 2,
  borderColor: '#eeeeee',
  borderStyle: 'dashed',
  backgroundColor: '#fafafa',
  color: '#bdbdbd',
  outline: 'none',
  transition: 'border .24s ease-in-out',
  // minWidth: "80vw"
};

const Base32Subdomain = (props) => {
  // const [base32Url, setBase32Url] = useState("");
  const [skyfileUrlTerm, setSkyfileUrlTerm] = useState('');
  const [base32SubdomainUrl, base32SubdomainLookup] = useBase32Subdomain();
  const skyfileUrl = useDebounce(skyfileUrlTerm, 500);

  useEffect(() => {
    base32SubdomainLookup(skyfileUrl);
    // let skylink = parseSkylink(skyfileUrl);
    // let portalName = "";
    // let protocol;

    // if (skylink) {
    //   try {
    //     const url = new URL(skyfileUrl);
    //     console.log(url);
    //     portalName = url.hostname;
    //     protocol = url.protocol;
    //   } catch (e) {
    //     console.log("url error");
    //     portalName = "siasky.net";
    //     protocol = "https:";
    //   }

    //   let onlyPath = parseSkylink(skyfileUrl, { onlyPath: true });
    //   console.log(portalName);

    //   setBase32Url(
    //     `${protocol}//${convertSkylinkToBase32(
    //       skylink
    //     )}.${portalName}${onlyPath}`
    //   );
    // }
  }, [skyfileUrl]);

  return (
    <div className="PortalAlternatives-container">
      <div className="baseStyle" style={baseStyle}>
        <Title level={5} type="secondary">
          Generate a Base32 Subdomain Skylink
        </Title>
        <Input
          placeholder="Paste a Skylink or Skyfile URL"
          value={skyfileUrlTerm}
          onChange={(e) => setSkyfileUrlTerm(e.target.value)}
        />

        <div className="PortalAlternatives-links">
          <Divider />
          <Title level={5}>
            <Link href={base32SubdomainUrl} target="_blank">
              {base32SubdomainUrl}
            </Link>
          </Title>
        </div>
      </div>
    </div>
  );
};

export default Base32Subdomain;
