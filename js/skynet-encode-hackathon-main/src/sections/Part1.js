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

const step1_2 = `// Import the SkynetClient and a helper
import { SkynetClient } from 'skynet-js';

// We'll define a portal to allow for developing on localhost.
// When hosted on a skynet portal, SkynetClient doesn't need any arguments.
const portal = 'https://siasky.net/';

// Initiate the SkynetClient
const client = new SkynetClient(portal);`;

const step1_3 = `// Upload user's file and get backs descriptor for our Skyfile
const { skylink } = await client.uploadFile(file);

// skylinks start with 'sia://' and don't specify a portal URL
// we can generate URLs for our current portal though.
const skylinkUrl = client.getSkylinkUrl(skylink);

console.log('File Uploaded:', skylinkUrl);

// To use this later in our React app, save the URL to the state.
setFileSkylink(skylinkUrl);`;

const Part1 = ({ codeColor }) => {
  return (
    <>
      <Header
        as="h2"
        content="Part 1: Upload a File"
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
            <Divider vertical>1.1</Divider>
            <Grid.Column>
              <Segment>
                <SyntaxHighlighter language="console" style={codeColor}>
                  {'yarn add skynet-js'}
                </SyntaxHighlighter>
              </Segment>
            </Grid.Column>
            <Grid.Column>
              <Segment>Installs Skynet's Javascript SDK.</Segment>
            </Grid.Column>
          </Grid.Row>
          <Grid.Row>
            <Divider vertical>1.2</Divider>
            <Grid.Column>
              <Segment>
                <SyntaxHighlighter language="javascript" style={codeColor}>
                  {step1_2}
                </SyntaxHighlighter>
              </Segment>
            </Grid.Column>
            <Grid.Column>
              <Segment>
                Initializes the <code>SkynetClient</code> for making calls to
                Skynet.
                <br />
                <br />
                Here, we've used a default portal of 'http://siasky.net'. We'll
                come back and remove this before we deploy to allow our code to
                communicate with any portal it is served by.
              </Segment>
            </Grid.Column>
          </Grid.Row>
          <Grid.Row>
            <Divider vertical>1.3</Divider>
            <Grid.Column>
              <Segment>
                <SyntaxHighlighter language="javascript" style={codeColor}>
                  {step1_3}
                </SyntaxHighlighter>
              </Segment>
            </Grid.Column>
            <Grid.Column>
              <Segment>
                This code will be called when the "Send to Skynet" button is
                pressed.
                <br />
                <br />
                We'll be putting much of the Skynet code in this{' '}
                <code>handleSubmit</code> function. Typically you'd break up
                much of this functionality for better code organization and
                reuse.
                <br />
                <br />
                The we upload the image file from the form, and convert the
                skylink to a URL accessible from the current portal.
              </Segment>
            </Grid.Column>
          </Grid.Row>
          <Grid.Row>
            <Divider vertical>1.4</Divider>
            <Grid.Column>
              <Segment>
                <SyntaxHighlighter language="javascript" style={codeColor}>
                  {`console.log('Uploading file...');`}
                </SyntaxHighlighter>
              </Segment>
            </Grid.Column>
            <Grid.Column>
              <Segment>
                We'll uncomment this line just above so we can keep track of
                what's happening in our app using the console. Press{' '}
                <code>F12</code> at any time in your browser to activate the
                Developer Tools.
              </Segment>
            </Grid.Column>
          </Grid.Row>
        </Grid>
      </Container>
    </>
  );
};

export default Part1;
