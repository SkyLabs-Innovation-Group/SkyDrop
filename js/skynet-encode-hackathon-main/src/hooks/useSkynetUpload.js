import { useState } from "react";
import { SkynetClient } from "skynet-js";

const client = new SkynetClient("https://siasky.net");

const useSkynetUpload = () => {
  const [status, setStatus] = useState("");
  const [progress, setProgress] = useState(-1);
  const [skylink, setSkylink] = useState("");

  const onUploadProgress = (progress, { loaded, total }) => {
    return setProgress(Math.round(progress * 100));
  };

  const uploadFile = async (file) => {
    try {
      setStatus("uploading");
      setProgress(0);

      const response = await client.uploadFile(file, { onUploadProgress });

      const portalUrl = client.getSkylinkUrl(response.skylink);

      setSkylink(portalUrl);
      setStatus("completed");
      setProgress(100);
    } catch (error) {
      setStatus("error");
    }
  };

  return [skylink, status, progress, uploadFile];
};

export default useSkynetUpload;
