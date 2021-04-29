import React, { useCallback, useMemo } from 'react';
import { useDropzone } from 'react-dropzone';
import useSkynetUpload from '../hooks/useSkynetUpload';
import { Progress, Typography } from 'antd';
import { Transition } from 'semantic-ui-react';
// import { Progress } from 'semantic-ui-react';

import './DragUploader.css';

const { Title, Link } = Typography;

const DragUploader = (props) => {
  // const [selectedFile, setSelectedFile] = useState(null);
  // const [copied, setCopied] = useState(false);

  const [skylink, status, progress, uploadFile] = useSkynetUpload();

  const onDrop = useCallback((acceptedFiles) => {
    console.log(acceptedFiles);
    if (!!acceptedFiles[0]) {
      uploadFile(acceptedFiles[0]);
    }
  }, []);

  const {
    getRootProps,
    getInputProps,
    isDragActive,
    isDragAccept,
    isDragReject,
  } = useDropzone({ onDrop });

  const style = useMemo(
    () => ({
      ...baseStyle,
      ...(isDragActive ? activeStyle : {}),
      ...(isDragAccept ? acceptStyle : {}),
      ...(isDragReject ? rejectStyle : {}),
    }),
    [isDragActive, isDragReject, isDragAccept]
  );

  return (
    <div className="DragUploader-drag-box">
      {status !== 'completed' && (
        <div {...getRootProps({ style })}>
          <input {...getInputProps()} />
          {isDragActive ? (
            <Title level={4} type="secondary">
              Drop the file here to upload to Skynet...
            </Title>
          ) : (
            <Title level={4} type="secondary">
              Drag your file here, or click to select a file to upload.
            </Title>
          )}
        </div>
      )}

      {status === 'completed' && skylink && (
        <div className="baseStyle" style={baseStyle}>
          <Title level={5}>
            <Link href={skylink} target="_blank" rel="noopener noreferrer">
              {skylink}
            </Link>
          </Title>
          <Title level={5} type="secondary" copyable={{ text: skylink }}>
            Copy Skylink to Clipboard
          </Title>
        </div>
      )}

      {/* {status !== 'uploading' && ( */}
      <Transition visible={status === 'uploading'}>
        <div className="DragUploader-loader-box">
          <div className="DragUploader-progress-wrapper">
            {/* <Progress percent={progress} size="small">
              why text here?
            </Progress> */}
            <Progress
              className="DragUploader-progress"
              type="circle"
              width={75}
              percent={progress}
            />
          </div>
        </div>
      </Transition>
      {/* )} */}
    </div>
  );
};

export default DragUploader;

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
  minHeight: '90px',
  paddingTop: '26px',
};

const activeStyle = {
  borderColor: '#2196f3',
};

const acceptStyle = {
  borderColor: '#00e676',
};

const rejectStyle = {
  borderColor: '#ff1744',
};
