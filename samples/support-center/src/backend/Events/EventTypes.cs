namespace SupportCenter.Events;

public enum EventType
{
    UserChatInput,
    UserConnected,
    UserNewConversation,
    AgentNotification,
    Unknown,
    // Domain specific events
    CustomerInfoRequested,
    CustomerInfoRetrieved,
    QnARequested,
    QnARetrieved,
    DiscountRequested,
    DiscountRetrieved,
    InvoiceRequested,
    InvoiceRetrieved
}