import React, { useEffect, useState } from 'react';
// import axios from 'axios';
import JSONPretty from 'react-json-pretty';
import 'react-json-pretty/themes/monikai.css';

import { SkynetClient } from 'skynet-js';

const client = new SkynetClient('https://siasky.net');
// const client = new SkynetClient();

const FilePreviewer = ({ skylink }) => {
  const [metadata, setMetadata] = useState({});
  const [fileContentType, setFileContentType] = useState('');
  const [fileContents, setFileContents] = useState({});

  useEffect(() => {
    const fetchMetadata = async () => {
      try {
        setMetadata({});
        setFileContents({});
        setFileContentType('');

        const skynetMetadata = await client.getMetadata(skylink);
        setMetadata(skynetMetadata);

        let contenttype = '';

        if (Object.keys(skynetMetadata['subfiles']).length === 1) {
          const subfile = Object.keys(skynetMetadata['subfiles'])[0];

          console.log(subfile);

          contenttype = skynetMetadata['subfiles'][subfile]['contenttype'];

          setFileContentType(contenttype);
          console.log(contenttype);
          console.log(fileContentType);
        }

        if (
          (contenttype.includes('json') || contenttype.includes('text')) &&
          skynetMetadata['length'] < 400000
        ) {
          const skynetFileContents = await client.requestFile(skylink);
          console.log(typeof skynetFileContents);
          console.log(skynetFileContents);
          setFileContents(skynetFileContents);
        }
      } catch (error) {
        console.log(error);
      }
    };

    if (skylink) {
      fetchMetadata();
    }
  }, [skylink, fileContentType]);

  return (
    <>
      <div>
        {Object.keys(metadata).length > 0 && (
          <JSONPretty id="json-pretty" data={metadata}></JSONPretty>
        )}
      </div>

      <div>
        {Object.keys(fileContents).length > 0 && (
          <>
            <h4>File Contents:</h4>
            <JSONPretty id="json-pretty" data={fileContents}></JSONPretty>
          </>
        )}
      </div>
    </>
  );
};

export default FilePreviewer;
