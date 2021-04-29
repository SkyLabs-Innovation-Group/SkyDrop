import { Container, Grid, Header, Card } from 'semantic-ui-react';
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

const Challenges = () => {
  return (
    <>
      <Header
        as="h2"
        content="Challenges"
        style={style.h2}
        textAlign="center"
      />

      <Container>
        <Grid relaxed columns={2}>
          <Grid.Column>
            <Header
              as="h3"
              content="Easy"
              style={style.h3}
              textAlign="center"
            />
            <Card
              fluid
              header="Host Your App on Skynet"
              meta="Prizes: 100k Siacoin pool"
              description="Use Skynet in your project. You can store your application or user data on Skynet, or just upload your web application’s front-end to Skynet."
            />
          </Grid.Column>
          <Grid.Column>
            <Header
              as="h3"
              content="Medium"
              style={style.h3}
              textAlign="center"
            />
            <Card
              fluid
              header="Contribute to the Skapp Ecosystem"
              meta="Prizes: 1 Winner x 200k Siacoins, 2 Runner-ups x 75k Siacoins"
              description="Build a Skapp (Skynet web application that doesn’t rely on centralized services) which utilizes decentralized login, an HNS domain name, and SkyDB for storing mutable application data. User-focused ideas and interoperability with other skapps in the ecosystem is a plus."
            />
          </Grid.Column>
        </Grid>
      </Container>
    </>
  );
};

export default Challenges;
