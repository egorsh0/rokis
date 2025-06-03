using System.ComponentModel;

namespace idcc.Infrastructures;

public enum MessageCode
{
    InternalServerError,
    
    [Description("User not found")]
    USER_NOT_FOUND,
    
    [Description("Invalid password")]
    INVALID_PASSWORD,
    [Description("Role is not found")]
    LOGIN_FINISHED,
    
    [Description("Role is not found")]
    ROLE_NOT_FOUND,
    [Description("Role is not correct")]
    ROLE_NOT_CORRECT,
    
    [Description("Either company not found")]
    COMPANY_NOT_FOUND,
    [Description("Either employee not found")]
    EMPLOYEE_NOT_FOUND,
    [Description("Either person not found")]
    PERSON_NOT_FOUND,
    
    [Description("The data is changed")]
    UPDATE_IS_FINISHED,
    [Description("The data has errors")]
    UPDATE_HAS_ERRORS,
    [Description("Nothing to update")]
    NOTHING_TO_UPDATE,
    
    [Description("Employee is attached")]
    EMPLOYEE_ATTACHED,
    
    [Description("Email address already exists")]
    EMAIL_ALREADY_EXISTS,
    [Description("Inn already exists")]
    INN_ALREADY_EXISTS,
    [Description("Email address or INN already exists")]
    EMAIL_OR_INN_ALREADY_EXISTS,
    
    [Description("Session is started")]
    SESSION_IS_STARTED,
    [Description("Session not found")]
    SESSION_IS_NOT_EXIST,
    [Description("Session is active")]
    SESSION_HAS_ACTIVE,
    [Description("Session is finished")]
    SESSION_IS_FINISHED,
    
    [Description("Items array must not be empty")]
    ORDER_SHOULD_HAS_ITEMS,
    [Description("Order not found")]
    ORDER_NOT_FOUND,
    [Description("Order is paid")]
    ORDER_PAID,
    [Description("The order was paid")]
    ORDER_IS_MARKED,
    
    [Description("Registered has errors")]
    REGISTER_HAS_ERRORS,
    [Description("Registered successfully")]
    REGISTER_IS_FINISHED,
    
    [Description("Token not found")]
    TOKEN_NOT_FOUND,
    [Description("Token not bound")]
    TOKEN_NOT_BOUND,
    [Description("Token is forbidden")]
    TOKEN_IS_FORBIDDEN,
    
    [Description("Company has not employee")]
    COMPANY_HAS_NOT_EMPLOYEE,
    [Description("Token successfully bound")]
    BIND_IS_FINISHED,
    
    [Description("Passwords do not match")]
    PASSWORD_DO_NOT_MATCH,
    [Description("Password reset successfully")]
    PASSWORD_RESET_SUCCESSFUL,
    PASSWORD_RESET_FAILED,
    
    [Description("It was not possible to get a random topic")]
    GET_RANDOM_TOPIC,
    TOPIC_IS_NULL,
    [Description("The topic is closed")]
    TOPIC_IS_CLOSED,
    
    [Description("The question was not found")]
    QUESTION_IS_NOT_EXIST,
    [Description("The question has multiply option")]
    QUESTION_IS_MULTIPLY,
    [Description("The answer is not tied to the question")]
    ANSWER_ID_NOT_FOUND,
    [Description("The answer was sending")]
    ANSWER_IS_SEND,
    [Description("Calculate is finished")]
    CALCULATE_IS_FINISHED,
    
    GRADE_TIMES_IS_NULL,
    GRADE_WEIGHT_IS_NULL,
    
    MAILING_TEMPLATE_IS_NULL,
    
    EMAIL_IS_SEND,
    
    [Description("It was not possible to create a report")]
    REPORT_IS_FAILED,
    [Description("The report already exists")]
    REPORT_ALREADY_EXISTS,
    [Description("The report not found")]
    REPORT_NOT_FOUND
}