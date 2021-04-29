import React, { useState, useEffect } from 'react';
import useGenerateKeysFromSeed from '../hooks/useGenerateKeysFromSeed';
import useSkynsUri from '../hooks/useSkynsUri';
import { useRegistryRead, useRegistryWrite } from '../hooks/useRegistry';
import { Input, Typography, Button, Divider } from 'antd';
import JSONPretty from 'react-json-pretty';
import 'react-json-pretty/themes/monikai.css';
import { Transition, Container } from 'semantic-ui-react';

import './PortalAlternatives.css';

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

const Registry = (props) => {
  const [seed, setSeed] = useState('');
  const [dataKey, setDataKey] = useState('');
  const [skylink, setSkylink] = useState('');

  const [
    publicKey,
    privateKey,
    generateKeysFromSeed,
  ] = useGenerateKeysFromSeed();

  const [writeRegistryStatus, registryWrite] = useRegistryWrite();

  const [
    registryEntry,
    registrySignature,
    encodedSkylink,
    readRegistryStatus,
    registryRead,
  ] = useRegistryRead();

  const [skynsUri, skynsUriConverter] = useSkynsUri();

  useEffect(() => {
    generateKeysFromSeed(seed);
  }, [seed]);

  useEffect(() => {
    skynsUriConverter(publicKey, dataKey);
  }, [publicKey, dataKey]);

  return (
    <>
      <div className="PortalAlternatives-container">
        <div className="baseStyle" style={baseStyle}>
          <Title level={4} type="secondary">
            Using the Registry &amp; <code>skyns://</code>
          </Title>
          <Input.Password
            placeholder="Secure Seed for Generating Keys"
            value={seed}
            onChange={(e) => setSeed(e.target.value)}
          />
          <Divider />
          <Title level={5} type="secondary">
            Registry - Write Entry
          </Title>
          <Input
            placeholder="Private Key from Seed"
            value={privateKey}
            disabled
          />
          <Input
            placeholder="Data Key"
            value={dataKey}
            onChange={(e) => setDataKey(e.target.value)}
          />
          <Input
            placeholder="Skylink to Write in Registry Entry"
            value={skylink}
            onChange={(e) => setSkylink(e.target.value)}
          />
          <br />
          <Button
            type="primary"
            danger={writeRegistryStatus === 'error'}
            loading={writeRegistryStatus === 'setting'}
            onClick={() =>
              registryWrite(publicKey, privateKey, dataKey, skylink)
            }
          >
            Registry Write
          </Button>
          <Divider />
          <Title level={5}>Registry - Read Entry</Title>
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
          <br />
          <Title level={5} type="secondary" copyable={{ text: skynsUri }}>
            Copy <code>skyns</code> Record to Clipboard
          </Title>
          <Divider />
          <Button
            type="primary"
            danger={readRegistryStatus === 'error'}
            loading={readRegistryStatus === 'getting'}
            onClick={() => registryRead(publicKey, dataKey)}
          >
            Registry Read
          </Button>
          <br />
          <Link href={encodedSkylink} target="_blank">
            {encodedSkylink}
          </Link>
        </div>
      </div>
      {readRegistryStatus === 'completed' && registryEntry && (
        <Container>
          <JSONPretty
            id="json-pretty"
            data={{
              ...registryEntry,
              revision: registryEntry.revision.toString(),
            }}
          ></JSONPretty>
        </Container>
      )}
    </>
  );
};

export default Registry;
