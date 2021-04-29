import SyntaxHighlighter from 'react-syntax-highlighter';
import { Container, Grid, Header, Segment, Divider } from 'semantic-ui-react';

const style = {
  h1: {
    marginTop: '3em',
  },
  h2: {
    margin: '4em 0em 2em',
  },
  h3: {
    // marginTop: '2em',
    // padding: '2em 0em',
  },
  last: {
    marginBottom: '300px',
  },
};

const step2_1 = `// Create the text of an html file what will be uploaded to Skynet
// We'll use the skylink from Part 1 in the file to load our Skynet-hosted image.
const webPage = generateWebPage(name, skylinkUrl);

// Build our directory object, we're just including the file for our webpage.
const webDirectory = {
  'index.html': webPage,
  // 'couldList.jpg': moreFiles,
};

// Upload user's webpage
const { skylink: dirSkylink } = await client.uploadDirectory(
  webDirectory,
  'certificate'
);

// generate a URL for our current portal
const dirSkylinkUrl = client.getSkylinkUrl(dirSkylink);

console.log('Web Page Uploaded:', dirSkylinkUrl);

// To use this later in our React app, save the URL to the state.
setWebPageSkylink(dirSkylinkUrl);`;

const Part1 = ({ codeColor }) => {
  return (
    <>
      <Header
        as="h2"
        content="Part 2: Upload a Web Page"
        style={style.h2}
        textAlign="center"
      />

      <Container>
        <Grid verticalAlign="middle" relaxed columns={2}>
          <Grid.Row>
            <Grid.Column>
              <Header
                as="h3"
                content="Code"
                style={style.h3}
                textAlign="center"
              />
            </Grid.Column>
            <Grid.Column>
              <Header
                as="h3"
                content="Notes"
                style={style.h3}
                textAlign="center"
              />
            </Grid.Column>
          </Grid.Row>
          <Grid.Row>
            <Divider vertical>2.1</Divider>
            <Grid.Column>
              <Segment>
                <SyntaxHighlighter language="javascript" style={codeColor}>
                  {step2_1}
                </SyntaxHighlighter>
              </Segment>
            </Grid.Column>
            <Grid.Column>
              <Segment>
                Here we're invoking a function that "generates" an html file.
                Mostly we're passing a pieces of form data to an otherwise
                static file.
                <br />
                <br />
                Next, we create an object of files we want in our directory.
                <br />
                <br />
                Finally, we upload the directory to get its skylink and URL.
              </Segment>
            </Grid.Column>
          </Grid.Row>
          <Grid.Row>
            <Divider vertical>2.2</Divider>
            <Grid.Column>
              <Segment>
                <SyntaxHighlighter language="javascript" style={codeColor}>
                  {`console.log('Uploading the web page...');`}
                </SyntaxHighlighter>
              </Segment>
            </Grid.Column>
            <Grid.Column>
              <Segment>
                We'll uncomment this line just above so we can keep track of
                what's happening in our code.
              </Segment>
            </Grid.Column>
          </Grid.Row>
        </Grid>
      </Container>
    </>
  );
};

export default Part1;
