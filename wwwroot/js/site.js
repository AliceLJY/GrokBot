// JavaScript for GrokBot

// Function to scroll to bottom of messages container
window.scrollToBottom = function(element) {
    if (element) {
        element.scrollTop = element.scrollHeight;
    }
};
