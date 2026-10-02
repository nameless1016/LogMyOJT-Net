// tiny helper so the placement page can copy the approval link
window.logmyojt = {
  copy: async function (text) {
    try {
      await navigator.clipboard.writeText(text);
      return true;
    } catch {
      return false;
    }
  },
};
