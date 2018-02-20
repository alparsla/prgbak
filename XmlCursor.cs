using System;
using System.Collections.Generic;
using System.Xml;

namespace PrgBak
{
	internal class XmlCursor
	{
		struct XmlNodeBorrow
		{
			public XmlNode node;
			public bool borrow;
		}

		private XmlDocument xmldoc;
		private XmlNode curNode;
		private Stack<XmlNodeBorrow> stack;
		private bool borrow;

		internal XmlCursor(string xml)
		{
			this.xmldoc = new XmlDocument();
			this.xmldoc.LoadXml(xml);

			Reset();
		}

		internal bool MoveNext()
		{
			if (this.borrow)
			{
				this.borrow = false;
				return true;
			}

			if (!MoveUntilElement(this.curNode.NextSibling))
			{
				return false;
			}

			return true;
		}

		public bool MoveIn()
		{
			if (!this.curNode.HasChildNodes)
			{
				return false;
			}

			if (!MoveUntilElement(this.curNode.FirstChild))
			{
				return false;
			}

			this.borrow = true;
			return true;
		}

		private bool MoveUntilElement(XmlNode node)
		{
			if (node == null)
			{
				return false;
			}

			XmlNode cur = node;
			while (cur.NodeType != XmlNodeType.Element)
			{
				cur = cur.NextSibling;
				if (cur == null)
				{
					return false;
				}
			}

			this.curNode = cur;
			return true;
		}

		public bool MoveOut()
		{
			if (this.curNode.ParentNode == null)
			{
				return false;
			}

			this.curNode = this.curNode.ParentNode;
			return true;
		}

		public string Element
		{
			get
			{
				return this.curNode.Name.ToLowerInvariant();
			}
		}

		public bool IsElement(string elem)
		{
			return Element.Equals(elem.ToLowerInvariant());
		}

		public void UnexpectedElement()
		{
			throw new XmlException("Unexpected element " + this.Element);
		}

		public string Text
		{
			get
			{
				return this.curNode.InnerText;
			}
		}

		public IDictionary<string, string> GetAllAttributes()
		{
			IDictionary<string, string> dict = new Dictionary<string, string>();
			foreach (XmlAttribute attr in this.curNode.Attributes)
			{
				dict.Add(attr.Name, attr.Value);
			}
			return dict;
		}


		public string GetAttr(string name, string defVal)
		{
			foreach (XmlAttribute attr in this.curNode.Attributes)
			{ 
				if (name.ToLowerInvariant().Equals(attr.Name.ToLowerInvariant()))
				{
					return attr.Value;
				}
			}
			return defVal;
		}

		public string GetAttr(string name, bool must)
		{
			string val = GetAttr(name, null);
			if (val == null)
			{
				throw new XmlException("Element <" + this.Element + "> must have " + name + " attribute");
			}

			return val;
		}

		public string GetAttr(string name)
		{
			return GetAttr(name, null);
		}

		public int GetIntAttr(string name, bool must)
		{
			return int.Parse(GetAttr(name, true));
		}

		public int GetIntAttr(string name)
		{
			return int.Parse(GetAttr(name, "0"));
		}

		public int GetIntAttr(string name, int defVal)
		{
			return int.Parse(GetAttr(name, defVal.ToString()));
		}

		public bool IsAttr(string name)
		{
			return IsAttr(name, false);
		}

		public bool IsAttr(string name, bool defVal)
		{
			string val = GetAttr(name, null);
			if (val == null)
			{
				return defVal;
			}

			val = val.ToLowerInvariant();
			return val.Equals("true") ||
				      val.Equals("1") ||
				      val.Equals("on") ||
				      val.Equals("yes");
		}

		public void Push()
		{
			if (this.stack == null)
			{
				this.stack = new Stack<XmlNodeBorrow>();
			}

			XmlNodeBorrow xnb = new XmlNodeBorrow();
			xnb.node = this.curNode;
			xnb.borrow = this.borrow;
			this.stack.Push(xnb);
		}

		public void Pop()
		{
			XmlNodeBorrow xnb = this.stack.Pop();
			this.curNode = xnb.node;
			this.borrow = xnb.borrow;
		}

		public void Reset()
		{
			this.curNode = this.xmldoc.FirstChild;
			this.borrow = false;
		}
	}
}
