import React from 'react';
import { Container, Paper } from '@mui/material';
import RegisterForm from '../components/RegisterForm';

const RegisterPage = () => {
  return (
    <Container maxWidth="sm" sx={{ mt: 4 }}>
      <Paper elevation={3}>
        <RegisterForm />
      </Paper>
    </Container>
  );
};

export default RegisterPage; 