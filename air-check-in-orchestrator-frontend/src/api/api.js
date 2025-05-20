import axiosOrchestrator from "./instances/orchestrator";
import axiosRegistration from "./instances/registration";
import axiosBaggage from "./instances/baggage";
import axiosPassenger from "./instances/passenger";
import axiosSession from "./instances/session";

export const login = (username, password) =>
  axiosOrchestrator.post(`/auth/login`, null, {
    params: { username, password },
  });

export const checkInPassenger = (data) =>
  axiosOrchestrator.post(`/orchestrator/checkin`, data);

export const addBaggage = (data) =>
  axiosOrchestrator.post(`/orchestrator/baggage`, data);

export const registerSession = (dynamicId) =>
  axiosSession.post(`/session/register`, null, {
    params: { dynamicId },
  });

export const validateSession = (dynamicId) =>
  axiosSession.get(`/session/validate`, {
    params: { dynamicId },
  });

export const authenticate = (login, pwd) =>
  axiosRegistration.post(`/registration/authenticate`, { login, pwd });

export const searchOrder = (request) =>
  axiosRegistration.post(`/registration/order`, request);

export const reserveSeat = (request) =>
  axiosRegistration.post(`/registration/reserve`, request);

export const registerFree = (request) =>
  axiosRegistration.post(`/registration/registerFree`, request);

export const registerPaid = (request) =>
  axiosRegistration.post(`/registration/registerPaid`, request);

export const simulatePayment = (dynamicId, passengerId, departureId, amount) =>
  axiosRegistration.post(`/registration/simulatePayment`, null, {
    params: { dynamicId, passengerId, departureId, amount },
  });

export const getAllowance = (dynamicId, orderId, passengerId) =>
  axiosBaggage.get(`/baggage/allowance`, {
    params: { dynamicId, orderId, passengerId },
  });

export const registerBaggage = (request) =>
  axiosBaggage.post(`/baggage/register`, request);

export const cancelBaggage = (request) =>
  axiosBaggage.post(`/baggage/cancel`, request);

export const simulateBaggagePayment = (
  dynamicId,
  passengerId,
  departureId,
  amount
) =>
  axiosBaggage.post(`/baggage/simulatePayment`, null, {
    params: { dynamicId, passengerId, departureId, amount },
  });

///Admin API for Baggage
export const getAllBaggagePayments = () =>
  axiosBaggage.get(`/admin/baggage/payments`);

export const getBaggagePaymentById = (id) =>
  axiosBaggage.get(`/admin/baggage/payments/${id}`);

export const createBaggagePayment = (payment) =>
  axiosBaggage.post(`/admin/baggage/payments`, payment);

export const updateBaggagePayment = (id, updated) =>
  axiosBaggage.put(`/admin/baggage/payments/${id}`, updated);

export const deleteBaggagePayment = (id) =>
  axiosBaggage.delete(`/admin/baggage/payments/${id}`);

export const getAllRegistrations = () =>
  axiosBaggage.get(`/admin/baggage/registrations`);

export const getRegistrationById = (id) =>
  axiosBaggage.get(`/admin/baggage/registrations/${id}`);

export const createRegistration = (registration) =>
  axiosBaggage.post(`/admin/baggage/registrations`, registration);

export const updateRegistration = (id, updated) =>
  axiosBaggage.put(`/admin/baggage/registrations/${id}`, updated);

export const deleteRegistration = (id) =>
  axiosBaggage.delete(`/admin/baggage/registrations/${id}`);

export const getAllPaidOptions = () =>
  axiosBaggage.get(`/admin/baggage/options`);

export const getPaidOptionById = (id) =>
  axiosBaggage.get(`/admin/baggage/options/${id}`);

export const createPaidOption = (option) =>
  axiosBaggage.post(`/admin/baggage/options`, option);

export const updatePaidOption = (id, updated) =>
  axiosBaggage.put(`/admin/baggage/options/${id}`, updated);

export const deletePaidOption = (id) =>
  axiosBaggage.delete(`/admin/baggage/options/${id}`);
///

export const getPassengers = () => axiosPassenger.get(`/passenger`);

export const getPassenger = (id) => axiosPassenger.get(`/passenger/${id}`);

export const createPassenger = (dto) => axiosPassenger.post(`/passenger`, dto);

export const updatePassenger = (id, dto) =>
  axiosPassenger.put(`/passenger/${id}`, dto);

export const deletePassenger = (id) =>
  axiosPassenger.delete(`/passenger/${id}`);

/// Admin API for Passenger
export const getAllAdminPassengers = () =>
  axiosPassenger.get(`/admin/passengers`);

export const getAdminPassengerById = (id) =>
  axiosPassenger.get(`/admin/passengers/${id}`);

export const createAdminPassenger = (data) =>
  axiosPassenger.post(`/admin/passengers`, data);

export const updateAdminPassenger = (id, data) =>
  axiosPassenger.put(`/admin/passengers/${id}`, data);

export const deleteAdminPassenger = (id) =>
  axiosPassenger.delete(`/admin/passengers/${id}`);

/// Admin API for Registration - Payments, Registrations, Reservations
export const getAllPayments = () =>
  axiosRegistration.get(`/admin/registration/payments`);

export const getPaymentById = (id) =>
  axiosRegistration.get(`/admin/registration/payments/${id}`);

export const createPayment = (payment) =>
  axiosRegistration.post(`/admin/registration/payments`, payment);

export const updatePayment = (id, updated) =>
  axiosRegistration.put(`/admin/registration/payments/${id}`, updated);

export const deletePayment = (id) =>
  axiosRegistration.delete(`/admin/registration/payments/${id}`);

export const getAllRegistrationsAdmin = () =>
  axiosRegistration.get(`/admin/registration/registrations`);

export const getRegistrationByIdAdmin = (id) =>
  axiosRegistration.get(`/admin/registration/registrations/${id}`);

export const createRegistrationAdmin = (data) =>
  axiosRegistration.post(`/admin/registration/registrations`, data);

export const updateRegistrationAdmin = (id, data) =>
  axiosRegistration.put(`/admin/registration/registrations/${id}`, data);

export const deleteRegistrationAdmin = (id) =>
  axiosRegistration.delete(`/admin/registration/registrations/${id}`);

export const getAllReservations = () =>
  axiosRegistration.get(`/admin/registration/reservations`);

export const getReservationById = (id) =>
  axiosRegistration.get(`/admin/registration/reservations/${id}`);

export const createReservation = (data) =>
  axiosRegistration.post(`/admin/registration/reservations`, data);

export const updateReservation = (id, data) =>
  axiosRegistration.put(`/admin/registration/reservations/${id}`, data);

export const deleteReservation = (id) =>
  axiosRegistration.delete(`/admin/registration/reservations/${id}`);

/// Admin API for Session
export const getAllSessions = () => axiosSession.get(`/admin/session`);

export const getSessionById = (id) => axiosSession.get(`/admin/session/${id}`);

export const createSession = (data) =>
  axiosSession.post(`/admin/session`, data);

export const updateSession = (id, data) =>
  axiosSession.put(`/admin/session/${id}`, data);

export const deleteSession = (id) =>
  axiosSession.delete(`/admin/session/${id}`);
