import axios from 'axios';

const ORCHESTRATOR_URL = process.env.REACT_APP_ORCHESTRATOR_URL || 'http://localhost:8080/api';
const REGISTRATION_URL = process.env.REACT_APP_REGISTRATION_URL || 'http://localhost:8083/api';
const BAGGAGE_URL = process.env.REACT_APP_BAGGAGE_URL || 'http://localhost:8081/api';
const PASSENGER_URL = process.env.REACT_APP_PASSENGER_URL || 'http://localhost:8082/api';
const SESSION_URL = process.env.REACT_APP_SESSION_URL || 'http://localhost:8084/api';

// -------------------- ORCHESTRATOR --------------------

export const login = (username, password) =>
    axios.post(`${ORCHESTRATOR_URL}/auth/login`, null, {
        params: { username, password }
    });

export const checkInPassenger = (data, token) =>
    axios.post(`${ORCHESTRATOR_URL}/orchestrator/checkin`, data, {
        headers: { Authorization: `Bearer ${token}` }
    });

export const addBaggage = (data, token) =>
    axios.post(`${ORCHESTRATOR_URL}/orchestrator/baggage`, data, {
        headers: { Authorization: `Bearer ${token}` }
    });

// -------------------- SESSION --------------------

export const registerSession = (dynamicId) =>
    axios.post(`${SESSION_URL}/session/register`, null, {
        params: { dynamicId }
    });

export const validateSession = (dynamicId) =>
    axios.get(`${SESSION_URL}/session/validate`, {
        params: { dynamicId }
    });

// -------------------- REGISTRATION --------------------

export const authenticate = (login, pwd) =>
    axios.post(`${REGISTRATION_URL}/registration/authenticate`, { login, pwd });

export const searchOrder = (request) =>
    axios.post(`${REGISTRATION_URL}/registration/order`, request);

export const reserveSeat = (request) =>
    axios.post(`${REGISTRATION_URL}/registration/reserve`, request);

export const registerFree = (request) =>
    axios.post(`${REGISTRATION_URL}/registration/registerFree`, request);

export const registerPaid = (request) =>
    axios.post(`${REGISTRATION_URL}/registration/registerPaid`, request);

export const simulatePayment = (dynamicId, passengerId, departureId, amount) =>
    axios.post(`${REGISTRATION_URL}/registration/simulatePayment`, null, {
        params: { dynamicId, passengerId, departureId, amount }
    });

// -------------------- BAGGAGE --------------------

export const getAllowance = (dynamicId, orderId, passengerId) =>
    axios.get(`${BAGGAGE_URL}/baggage/allowance`, {
        params: { dynamicId, orderId, passengerId }
    });

export const registerBaggage = (request) =>
    axios.post(`${BAGGAGE_URL}/baggage/register`, request);

export const cancelBaggage = (request) =>
    axios.post(`${BAGGAGE_URL}/baggage/cancel`, request);

export const simulateBaggagePayment = (dynamicId, passengerId, departureId, amount) =>
    axios.post(`${BAGGAGE_URL}/baggage/simulatePayment`, null, {
        params: { dynamicId, passengerId, departureId, amount }
    });

// -------------------- PASSENGERS --------------------

export const getPassengers = () =>
    axios.get(`${PASSENGER_URL}/passenger`);

export const getPassenger = (id) =>
    axios.get(`${PASSENGER_URL}/passenger/${id}`);

export const createPassenger = (dto) =>
    axios.post(`${PASSENGER_URL}/passenger`, dto);

export const updatePassenger = (id, dto) =>
    axios.put(`${PASSENGER_URL}/passenger/${id}`, dto);

export const deletePassenger = (id) =>
    axios.delete(`${PASSENGER_URL}/passenger/${id}`);
