import React, { useState, useEffect } from 'react';
import useSkyfileMetadata from '../hooks/useSkyfileMetadata';
import useDebounce from '../hooks/useDebounce';
import { Input, List, Typography, Divider } from 'antd';
import JSONPretty from 'react-json-pretty';
import 'react-json-pretty/themes/monikai.css';
import { Transition, Container } from 'semantic-ui-react';

// import './FilePreview.css';
import { parseSkylink } from 'skynet-js';

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

const FilePreview = (props) => {
  const [skyfileUrlTerm, setSkyfileUrlTerm] = useState('');
  const [metadata, status, getMetadata] = useSkyfileMetadata();
  const skyfileUrl = useDebounce(skyfileUrlTerm, 500);

  useEffect(() => {
    if (parseSkylink(skyfileUrl)) {
      getMetadata(parseSkylink(skyfileUrl));
    }
  }, [skyfileUrl]);

  return (
    <>
      <div className="PortalAlternatives-container">
        <div className="baseStyle" style={baseStyle}>
          <Title level={5} type="secondary">
            Paste your Skyfile URL To Preview Its Contents
          </Title>
          <Input
            placeholder="Skyfile URL"
            value={skyfileUrlTerm}
            onChange={(e) => setSkyfileUrlTerm(e.target.value)}
            disabled={status === 'checking'}
          />

          <div className="PortalAlternatives-links">
            <Divider />
          </div>
        </div>
      </div>
      <Transition visible={status === 'completed'}>
        <Container>
          <JSONPretty id="json-pretty" data={metadata}></JSONPretty>
        </Container>
      </Transition>
    </>
  );
};

export default FilePreview;
