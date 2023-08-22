---
name: "\U0001F6E0️ API proposal"
about: Propose a change/addition or fix to the public API interface
title: ''
labels: enhancement
assignees: ''

---

**Which API members are affected by this request? Include their full name, assembly and parameter types names, if applyable:**
*I want to change the `Sisk.Core.HttpServer.Emit(int, @config, @host, @router)`, because I think this method API design is ugly.*

**How do you think it would be better?**
*I think it should be made clear that this method shouldn't be used in production, specifying it on it's summary or public docs.*

**What motivated you to think that?**
*I've seen some colleagues use this method for an application in production.*
