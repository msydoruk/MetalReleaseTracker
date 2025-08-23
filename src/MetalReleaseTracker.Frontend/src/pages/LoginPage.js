import React from 'react';
import { Container, Paper } from '@mui/material';
import LoginForm from '../components/LoginForm';

const LoginPage = () => {
  return (
    <Container maxWidth="sm" sx={{ mt: 4 }}>
      <Paper elevation={3}>
        <LoginForm />
      </Paper>
    </Container>
  );
};

export default LoginPage; 