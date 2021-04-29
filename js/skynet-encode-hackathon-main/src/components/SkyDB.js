import React, { useState, useEffect } from 'react';
import useGenerateKeysFromSeed from '../hooks/useGenerateKeysFromSeed';
import { useSkyDBSetJson, useSkyDBGetJson } from '../hooks/useSkyDB';
// import useBase32Subdomain from "../hooks/useBase32Subdomain";
import { Input, Typography, Button, Divider } from 'antd';
import JSONPretty from 'react-json-pretty';
import 'react-json-pretty/themes/monikai.css';
import { Transition, Container } from 'semantic-ui-react';

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
  // minWidth: '80vw',
};

const SkyDB = (props) => {
  const [seed, setSeed] = useState('');
  const [dataKey, setDataKey] = useState('');
  const [message, setMessage] = useState('');
  const [
    publicKey,
    privateKey,
    generateKeysFromSeed,
  ] = useGenerateKeysFromSeed();
  const [setJsonStatus, setJSON] = useSkyDBSetJson();
  const [skyDbEntry, getJsonStatus, getJSON] = useSkyDBGetJson();

  // const [base32Url, setBase32Url] = useState("");
  // const [skyfileUrlTerm, setSkyfileUrlTerm] = useState("");
  // const [base32SubdomainUrl, base32SubdomainLookup] = useBase32Subdomain();
  // const skyfileUrl = useDebounce(skyfileUrlTerm, 500);

  useEffect(() => {
    generateKeysFromSeed(seed);
  }, [seed]);

  return (
    <>
      <div className="PortalAlternatives-container">
        <div className="baseStyle" style={baseStyle}>
          <Title level={4} type="secondary">
            Using SkyDB
          </Title>
          <Input.Password
            placeholder="Secure Seed for Generating Keys"
            value={seed}
            onChange={(e) => setSeed(e.target.value)}
          />
          <Divider />
          <Title level={5} type="secondary">
            SkyDB setJSON
          </Title>
          <Input
            placeholder="Private Key from Seed"
            value={privateKey}
            disabled
            // onChange={(e) => setSeed(e.target.value)}
          />
          <Input
            placeholder="Data Key"
            value={dataKey}
            onChange={(e) => setDataKey(e.target.value)}
          />
          <Input
            placeholder="Message to save in 'message' field"
            value={message}
            onChange={(e) => setMessage(e.target.value)}
          />
          <Button
            type="primary"
            loading={setJsonStatus === 'setting'}
            onClick={() => setJSON(privateKey, dataKey, message)}
          >
            SkyDB setJson
          </Button>
          <Divider />
          <Title level={5}>SkyDB getJSON</Title>
          <Input
            placeholder="Public Key from Seed"
            value={publicKey}
            disabled
            onChange={(e) => setSeed(e.target.value)}
          />
          <Input
            placeholder="Data Key"
            value={dataKey}
            onChange={(e) => setDataKey(e.target.value)}
          />
          <Button
            type="primary"
            loading={getJsonStatus === 'getting'}
            // disabled={setJsonStatus !== 'completed'}
            onClick={() => getJSON(publicKey, dataKey)}
          >
            SkyDB getJson
          </Button>
          {/* <Title level={5}>{JSON.stringify(skyDbEntry)}</Title> */}
          {/* <Link href={base32SubdomainUrl} target="_blank">
              {base32SubdomainUrl}
            </Link> */}
        </div>
      </div>
      <Transition visible={getJsonStatus === 'completed'}>
        <Container>
          <JSONPretty id="json-pretty" data={skyDbEntry}></JSONPretty>
        </Container>
      </Transition>
    </>
  );
};

export default SkyDB;
